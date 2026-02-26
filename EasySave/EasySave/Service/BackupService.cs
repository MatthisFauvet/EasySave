using EasyLog;
using EasyLog.entity;
using EasyLog.utils;
using EasySave.Model;
using EasySave.Repository;
using EasySave.Service;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

// Service d'exécution des sauvegardes
public class BackupService : IBackupService
{
    private readonly Logger _logger;

    private IBackupRepository _backupRepository;
    private readonly JsonHistoryRepository _historyRepository = new();

    // Verrous par destination → évite les conflits d'écriture parallèle
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _destinationLocks = new();

    // Processus métier bloquant les sauvegardes
    private readonly string _businessProcessName = "CalculatorApp";

    public int MaxBandwidthKbps { get; set; } = 0;

    // Dossier des logs → chemin absolu si déjà absolu, sinon relatif à l'AppBase
    public string LogsDirectory { get; set; } = "logs";
    public string LogFileType { get; set; } = "JSON";
    private string ResolveLogsDirectory() =>
        Path.IsPathRooted(LogsDirectory)
            ? LogsDirectory
            : Path.Combine(AppContext.BaseDirectory, LogsDirectory);

    public BackupService(Logger logger)
    {
        _logger = logger;
        InitializeBackupRepository();
    }

    public BackupService()
    {
        _logger = new Logger();
        InitializeBackupRepository();
    }

    private void InitializeBackupRepository()
    {
        _backupRepository = new JsonBackupRepository();
    }

    // Retourne true si le processus métier est actif
    private bool IsBusinessSoftwareRunning()
    {
        try
        {
            return Process.GetProcessesByName(_businessProcessName).Any();
        }
        catch (Exception ex)
        {
            _logger.Log(
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"Error checking business software process: {ex.Message}"
                ),
                LogType.Error
            );
            return false;
        }
    }

    // Exécute des backups séquentiellement → true si tous réussissent
    public bool ExecuteBackup(List<Backup> backups)
    {
        bool isSuccessful = true;
        List<int> unvalidBackUps = new List<int>();

        _logger.InitDailyWriter(ResolveLogsDirectory(), LogFileType);
        _logger.Log(
            DictionaryManager.SingleStringToDictionary(
                "message",
                "Starting execution of backups."
            ),
            LogType.Info
        );

        foreach (Backup backup in backups)
        {
            try
            {
                ExecuteSingleBackup(backup);

                _logger.Log(
                    DictionaryManager.SingleStringToDictionary(
                        "message",
                        $"Backup (ID : {backup.Id}) completed successfully."
                    ),
                    LogType.Info
                );
            }
            catch (Exception ex)
            {
                _logger.Log(
                    DictionaryManager.SingleStringToDictionary(
                        "message",
                        $"Backup (ID : {backup.Id}) failed : {ex.Message}"
                    ),
                    LogType.Error
                );

                unvalidBackUps.Add(backup.Id);
                isSuccessful = false;
            }
        }

        if (!isSuccessful)
        {
            _logger.Log(
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"The following backup(s) failed to execute: {string.Join(", ", unvalidBackUps)}"
                ),
                LogType.Error
            );
        }

        _logger.Log(
            DictionaryManager.SingleStringToDictionary(
                "message",
                "Finished execution of backups."
            ),
            LogType.Info
        );

        return isSuccessful;
    }

    public void CreateBackup(BackupCreateRequest backupCreateRequest)
    {
        _backupRepository.CreateBackup(backupCreateRequest);
    }

    public void RemoveBackup(Backup backup)
    {
        _backupRepository.RemoveBackup(backup.Id);
    }

    public Backup? GetBackupById(int backupId)
    {
        return _backupRepository.GetBackupById(backupId);
    }

    public List<Backup> GetBackups(int pageIndex, int pageSize)
    {
        return _backupRepository.GetAllBackups(); 
    }

    public void UpdateBackup(Backup backup)
    {
        _backupRepository.UpdateBackup(backup);
    }

    // Exécute les backups en parallèle, prioritaires d'abord puis normaux → true si tous réussissent
    public async Task<bool> ExecuteBackupAsync(List<Backup> backups, Action<Backup>? onBackupUpdate = null)
    {
        var failedIds = new ConcurrentBag<int>();

        _logger.InitDailyWriter(ResolveLogsDirectory(), LogFileType);
        _logger.Log(
            DictionaryManager.SingleStringToDictionary("message", "Starting parallel execution of backups."),
            LogType.Info);

        async Task RunOne(Backup backup)
        {
            backup.Status   = BackupStatus.InProgress;
            backup.Progress = 0;
            onBackupUpdate?.Invoke(backup);

            var startTime          = DateTime.Now;
            var executionStopwatch = Stopwatch.StartNew();
            long sizeBytes   = 0;
            int  fileCount   = 0;
            bool hasWarnings = false;

            var destinationLock = GetDestinationLock(backup.DestinationFilePath);
            _logger.Log(
                DictionaryManager.SingleStringToDictionary("message",
                    $"Backup (ID: {backup.Id}) waiting for destination lock on '{backup.DestinationFilePath}'."),
                LogType.Info);

            await destinationLock.WaitAsync();
            try
            {
                _logger.Log(
                    DictionaryManager.SingleStringToDictionary("message",
                        $"Backup (ID: {backup.Id}) acquired destination lock, starting execution."),
                    LogType.Info);

                (sizeBytes, fileCount, hasWarnings) = await Task.Run(() => ExecuteSingleBackup(backup, progress =>
                {
                    backup.Progress = progress;
                    onBackupUpdate?.Invoke(backup);
                }));

                executionStopwatch.Stop();
                backup.Status             = BackupStatus.Completed;
                backup.Progress           = 100;
                backup.LastBackupDateTime = DateTime.Now;
                _logger.Log(
                    DictionaryManager.SingleStringToDictionary("message", $"Backup (ID: {backup.Id}) completed successfully."),
                    LogType.Info);

                _historyRepository.AddEntry(new HistoryEntry
                {
                    BackupName      = backup.Name,
                    SourcePath      = backup.SourceFilePath,
                    DestinationPath = backup.DestinationFilePath,
                    StartTime       = startTime,
                    DurationMs      = executionStopwatch.ElapsedMilliseconds,
                    TotalSizeBytes  = sizeBytes,
                    FileCount       = fileCount,
                    Status          = BackupStatus.Completed,
                    HasWarnings     = hasWarnings
                });
            }
            catch (Exception ex)
            {
                executionStopwatch.Stop();
                backup.Status = BackupStatus.Error;
                failedIds.Add(backup.Id);
                _logger.Log(
                    DictionaryManager.SingleStringToDictionary("message", $"Backup (ID: {backup.Id}) failed: {ex.Message}"),
                    LogType.Error);

                _historyRepository.AddEntry(new HistoryEntry
                {
                    BackupName      = backup.Name,
                    SourcePath      = backup.SourceFilePath,
                    DestinationPath = backup.DestinationFilePath,
                    StartTime       = startTime,
                    DurationMs      = executionStopwatch.ElapsedMilliseconds,
                    TotalSizeBytes  = sizeBytes,
                    FileCount       = fileCount,
                    Status          = BackupStatus.Error,
                    HasWarnings     = hasWarnings,
                    ErrorMessage    = ex.Message
                });
            }
            finally
            {
                destinationLock.Release();
            }

            try
            {
                _backupRepository.UpdateBackup(backup);
                onBackupUpdate?.Invoke(backup);
            }
            catch (Exception ex)
            {
                _logger.Log(
                    DictionaryManager.SingleStringToDictionary("message",
                        $"Backup (ID: {backup.Id}) post-execution update failed: {ex.Message}"),
                    LogType.Error);
            }
        }

        // 1. Prioritaires en parallèle
        var priorityBackups = backups.Where(b => b.IsPriority).ToList();
        if (priorityBackups.Count > 0)
            await Task.WhenAll(priorityBackups.Select(RunOne));

        // 2. Normaux en parallèle (démarrent après le groupe 1)
        var normalBackups = backups.Where(b => !b.IsPriority).ToList();
        if (normalBackups.Count > 0)
            await Task.WhenAll(normalBackups.Select(RunOne));

        if (!failedIds.IsEmpty)
            _logger.Log(
                DictionaryManager.SingleStringToDictionary("message", $"Failed backup(s): {string.Join(", ", failedIds)}"),
                LogType.Error);

        _logger.Log(
            DictionaryManager.SingleStringToDictionary("message", "Finished parallel execution of backups."),
            LogType.Info);

        return failedIds.IsEmpty;
    }

    public List<HistoryEntry> GetHistory() => _historyRepository.GetAll();

    // Retourne ou crée le SemaphoreSlim partagé pour un chemin de destination
    private static SemaphoreSlim GetDestinationLock(string destinationPath)
    {
        var key = Path.GetFullPath(destinationPath)
                      .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                      .ToLowerInvariant();
        return _destinationLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
    }

    // Copie les fichiers d'un backup (fichier ou dossier) → (totalBytes, nbFichiers, avertissements)
    private (long totalSizeBytes, int fileCount, bool hasWarnings) ExecuteSingleBackup(Backup backup, Action<double>? onProgress = null)
    {
        _logger.Log(
            DictionaryManager.SingleStringToDictionary("message",
                $"Starting backup (ID: {backup.Id}, Type: {backup.Type})"),
            LogType.Info);

        if (IsBusinessSoftwareRunning())
        {
            _logger.Log(
                DictionaryManager.SingleStringToDictionary("message",
                    $"Backup blocked: business software running ({_businessProcessName})"),
                LogType.Warning);
            throw new InvalidOperationException(
                $"Backup cannot execute: business software running ({_businessProcessName})");
        }

        // Résolution des fichiers sources
        FileInfo[] files;
        string     sourceBase;

        if (File.Exists(backup.SourceFilePath))
        {
            files      = [new FileInfo(backup.SourceFilePath)];
            sourceBase = Path.GetDirectoryName(backup.SourceFilePath)
                         ?? backup.SourceFilePath;
        }
        else if (Directory.Exists(backup.SourceFilePath))
        {
            var sourceDir = new DirectoryInfo(backup.SourceFilePath);
            var allFiles  = sourceDir.GetFiles("*", SearchOption.AllDirectories);

            // Séquentiel : fichiers modifiés depuis le dernier backup
            files = backup.Type == BackupType.Sequential && backup.LastBackupDateTime != default
                ? allFiles.Where(f => f.LastWriteTime > backup.LastBackupDateTime).ToArray()
                : allFiles;

            sourceBase = backup.SourceFilePath;
        }
        else
        {
            throw new FileNotFoundException($"Source not found: {backup.SourceFilePath}");
        }

        Directory.CreateDirectory(backup.DestinationFilePath);

        long totalSize     = files.Sum(f => f.Length);
        long processedSize = 0;
        bool hasWarnings   = false;

        foreach (FileInfo sourceFile in files)
        {
            string relativePath      = Path.GetRelativePath(sourceBase, sourceFile.FullName);
            string destinationFile   = Path.Combine(backup.DestinationFilePath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (MaxBandwidthKbps > 0)
                    CopyFileThrottled(sourceFile.FullName, destinationFile, MaxBandwidthKbps);
                else
                    CopyFileStream(sourceFile.FullName, destinationFile);
                stopwatch.Stop();

                _logger.Log(new Dictionary<string, string>
                {
                    { "sourcePath",      sourceFile.FullName },
                    { "destinationPath", destinationFile },
                    { "fileSize",        sourceFile.Length.ToString() },
                    { "transferTimeMs",  stopwatch.ElapsedMilliseconds.ToString() },
                    { "backupName",      backup.Name }
                });
            }
            catch (IOException ex)
            {
                stopwatch.Stop();
                hasWarnings = true;
                _logger.Log(new Dictionary<string, string>
                {
                    { "sourcePath",     sourceFile.FullName },
                    { "fileSize",       sourceFile.Length.ToString() },
                    { "transferTimeMs", stopwatch.ElapsedMilliseconds.ToString() },
                    { "backupName",     backup.Name },
                    { "error",          ex.Message }
                }, LogType.Warning);
                continue; // fichier verrouillé, ignoré
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.Log(new Dictionary<string, string>
                {
                    { "sourcePath",     sourceFile.FullName },
                    { "fileSize",       sourceFile.Length.ToString() },
                    { "transferTimeMs", stopwatch.ElapsedMilliseconds.ToString() },
                    { "backupName",     backup.Name },
                    { "error",          ex.Message }
                }, LogType.Error);
                throw;
            }

            processedSize += sourceFile.Length;
            double progress = totalSize > 0 ? (double)processedSize / totalSize * 100.0 : 100.0;
            onProgress?.Invoke(progress);

            if (IsBusinessSoftwareRunning())
            {
                _logger.Log(
                    DictionaryManager.SingleStringToDictionary("message",
                        $"Backup interrupted: business software detected after {sourceFile.Name}"),
                    LogType.Warning);
                throw new InvalidOperationException(
                    $"Backup interrupted: business software running ({_businessProcessName})");
            }
        }

        return (totalSize, files.Length, hasWarnings);
    }

    // Copie via stream avec FileShare.ReadWrite (fichiers ouverts tolérés)
    private static void CopyFileStream(string src, string dst)
    {
        using var input  = new FileStream(src, FileMode.Open,   FileAccess.Read,  FileShare.ReadWrite);
        using var output = new FileStream(dst, FileMode.Create, FileAccess.Write);
        input.CopyTo(output);
    }

    // Copie avec limitation de débit à maxKbps Ko/s
    private static void CopyFileThrottled(string src, string dst, int maxKbps)
    {
        const int chunkSize = 8192;
        double maxBytesPerMs = maxKbps * 1024.0 / 1000.0;

        using var input  = new FileStream(src, FileMode.Open,   FileAccess.Read,  FileShare.ReadWrite);
        using var output = new FileStream(dst, FileMode.Create, FileAccess.Write);

        var buffer = new byte[chunkSize];
        int read;
        var sw = Stopwatch.StartNew();
        long totalWritten = 0;

        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
            totalWritten += read;

            long expectedMs = (long)(totalWritten / maxBytesPerMs);
            long delay = expectedMs - sw.ElapsedMilliseconds;
            if (delay > 0)
                Thread.Sleep((int)delay);
        }
    }
}
