using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using EasyLog;
using EasyLog.entity;
using EasyLog.utils;
using EasySave.Model;
using EasySave.Repository;
using EasySave.Service;
using System.Text.Json;
using System.Text.RegularExpressions;
using EasyLog.writers;

public class BackupService : IBackupService
{
    private readonly Logger _logger;
    private Settings _settings;
    private IBackupRepository _backupRepository;

    private readonly StateService _stateService;
    private readonly string _businessProcessName = "CalculatorApp";

    private readonly ConcurrentDictionary<string, ManualResetEventSlim> _pauseGates
        = new ConcurrentDictionary<string, ManualResetEventSlim>();

    public void PauseBackup(Backup backup)
    {
        if (_pauseGates.TryGetValue(backup.Name, out ManualResetEventSlim? gate))
        {
            gate.Reset();
            _stateService.MarkPaused(backup.Name);
        }
    }

    public void ResumeBackup(Backup backup)
    {
        if (_pauseGates.TryGetValue(backup.Name, out ManualResetEventSlim? gate))
        {
            gate.Set();
            _stateService.MarkRunning(backup.Name);
        }
    }

    public BackupService(Logger logger)
    {
        _logger = logger;
        _stateService = new StateService();
        InitializeBackupRepository();
        LoadSettings();
    }

    public BackupService()
    {
        _logger = new Logger();
        _stateService = new StateService();
        InitializeBackupRepository();
        LoadSettings();
    }

    private void InitializeBackupRepository()
    {
        _backupRepository = new JsonBackupRepository();
    }

    private bool IsBusinessSoftwareRunning()
    {
        try
        {
            return Process.GetProcessesByName(_businessProcessName).Any();
        }
        catch (Exception ex)
        {
            _logger.Log(
                DictionaryManager.SingleStringToDictionary("message", $"Error checking business software process: {ex.Message}"),
                LogType.Error
            );
            return false;
        }
    }

    public bool ExecuteBackup(List<Backup> backups)
    {
        LoadSettings();

        ConcurrentBag<int> failedBackupIds = new ConcurrentBag<int>();
        using CancellationTokenSource cts = new CancellationTokenSource();

        using Logger executionLogger = new Logger();

        // The shared daily logger writes to local and/or Docker depending on the mode
        AddDailyLogWriters(executionLogger, "Execution of backups");

        executionLogger.Log(
            DictionaryManager.SingleStringToDictionary("message", "Starting execution of backups."),
            LogType.Info
        );

        List<Task> tasks = backups.Select(backup => Task.Run(() =>
        {
            try
            {
                ExecuteSingleBackup(backup, cts.Token, executionLogger);
                executionLogger.Log(
                    DictionaryManager.SingleStringToDictionary("message", $"Backup (ID: {backup.Id}) completed successfully."),
                    LogType.Info
                );
            }
            catch (OperationCanceledException)
            {
                _stateService.MarkCancelled(backup.Name);
                _pauseGates.TryRemove(backup.Name, out _);
                executionLogger.Log(
                    DictionaryManager.SingleStringToDictionary("message", $"Backup (ID: {backup.Id}) was cancelled — business software detected."),
                    LogType.Warning
                );
                failedBackupIds.Add(backup.Id);
            }
            catch (Exception ex)
            {
                _stateService.MarkError(backup.Name);
                _pauseGates.TryRemove(backup.Name, out _);
                executionLogger.Log(
                    DictionaryManager.SingleStringToDictionary("message", $"Backup (ID: {backup.Id}) failed: {ex.Message}"),
                    LogType.Error
                );
                failedBackupIds.Add(backup.Id);
            }
        }, cts.Token)).ToList();

        Task.WhenAll(tasks).Wait();

        bool isSuccessful = failedBackupIds.IsEmpty;

        if (!isSuccessful)
            executionLogger.Log(
                DictionaryManager.SingleStringToDictionary("message", $"The following backup(s) failed: {string.Join(", ", failedBackupIds)}"),
                LogType.Error
            );

        executionLogger.Log(
            DictionaryManager.SingleStringToDictionary("message", "Finished execution of backups."),
            LogType.Info
        );

        return isSuccessful;
    }

    private void ExecuteSingleBackup(Backup backup, CancellationToken token, Logger sharedLogger)
    {
        using Logger backupLogger = new Logger();

        // Per-backup log always goes to the backup destination folder (local, unchanged)
        AddFileWriter(backupLogger, backup.DestinationFilePath, $"Execution du backup {backup.Id}");

        void LogInfo(string message)
        {
            var entry = DictionaryManager.SingleStringToDictionary("message", message);
            backupLogger.Log(entry, LogType.Info);
            sharedLogger.Log(entry, LogType.Info);
        }

        void LogWarning(string message)
        {
            var entry = DictionaryManager.SingleStringToDictionary("message", message);
            backupLogger.Log(entry, LogType.Warning);
            sharedLogger.Log(entry, LogType.Warning);
        }

        void LogError(string message)
        {
            var entry = DictionaryManager.SingleStringToDictionary("message", message);
            backupLogger.Log(entry, LogType.Error);
            sharedLogger.Log(entry, LogType.Error);
        }

        void LogFile(Dictionary<string, string> logs, LogType type = LogType.Info)
        {
            backupLogger.Log(logs, type);
            sharedLogger.Log(logs, type);
        }

        LogInfo($"Starting backup execution (ID: {backup.Id}, Type: {backup.Type})");

        token.ThrowIfCancellationRequested();

        if (IsBusinessSoftwareRunning())
        {
            LogWarning($"Backup blocked before start — business software detected ({_businessProcessName})");
            throw new InvalidOperationException($"Backup cannot start — business software detected ({_businessProcessName})");
        }

        if (!Directory.Exists(backup.SourceFilePath))
        {
            LogError($"Source directory not found: {backup.SourceFilePath}");
            throw new DirectoryNotFoundException($"Source not found: {backup.SourceFilePath}");
        }

        Directory.CreateDirectory(backup.DestinationFilePath);

        DirectoryInfo sourceDirectory = new DirectoryInfo(backup.SourceFilePath);
        FileInfo[] files = sourceDirectory.GetFiles("*", SearchOption.AllDirectories);

        long totalSize  = files.Sum(f => f.Length);
        int  totalFiles = files.Length;

        ManualResetEventSlim pauseGate = new ManualResetEventSlim(initialState: true);
        _pauseGates[backup.Name] = pauseGate;

        _stateService.Initialize(
            name           : backup.Name,
            source         : backup.SourceFilePath,
            destination    : backup.DestinationFilePath,
            totalFiles     : totalFiles,
            totalSizeBytes : totalSize
        );

        foreach (FileInfo sourceFile in files)
        {
            pauseGate.Wait(token);
            token.ThrowIfCancellationRequested();

            string relativePath        = Path.GetRelativePath(backup.SourceFilePath, sourceFile.FullName);
            string destinationFilePath = Path.Combine(backup.DestinationFilePath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath)!);

            if (backup.Type == BackupType.Sequential)
            {
                FileInfo destFile = new FileInfo(destinationFilePath);
                if (destFile.Exists && sourceFile.LastWriteTime <= destFile.LastWriteTime)
                {
                    _stateService.IncrementProgress(backup.Name);
                    continue;
                }
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                sourceFile.CopyTo(destinationFilePath, true);
                stopwatch.Stop();

                DateTime backupTime = DateTime.Now;
                File.SetCreationTime(destinationFilePath, backupTime);
                File.SetLastWriteTime(destinationFilePath, backupTime);
                File.SetLastAccessTime(destinationFilePath, backupTime);

                Dictionary<string, string> logs = new Dictionary<string, string>
                {
                    { "sourcePath",      sourceFile.FullName },
                    { "destinationPath", destinationFilePath },
                    { "fileSize",        sourceFile.Length.ToString() },
                    { "transferTimeMs",  stopwatch.ElapsedMilliseconds.ToString() },
                    { "backupName",      backup.Name },
                    { "machine",         Environment.MachineName },
                };

                LogFile(logs);
                _stateService.IncrementProgress(backup.Name);

                if (IsBusinessSoftwareRunning())
                {
                    LogWarning($"Business software detected after copying {sourceFile.Name} — cancelling all backups.");
                    throw new OperationCanceledException(token);
                }
            }
            catch (OperationCanceledException) { throw; }
            catch
            {
                stopwatch.Stop();
                Dictionary<string, string> logs = new Dictionary<string, string>
                {
                    { "sourcePath",     sourceFile.FullName },
                    { "fileSize",       sourceFile.Length.ToString() },
                    { "transferTimeMs", stopwatch.ElapsedMilliseconds.ToString() },
                    { "backupName",     backup.Name },
                    { "machine",        Environment.MachineName },
                };
                LogFile(logs, LogType.Error);
            }
        }

        _stateService.MarkCompleted(backup.Name);
        _pauseGates.TryRemove(backup.Name, out _);
        pauseGate.Dispose();
    }

    // ── Log writer helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Adds a single file writer (JSON or XML) to the given logger for a specific path.
    /// Used for per-backup destination logs — always local.
    /// </summary>
    private void AddFileWriter(Logger logger, string path, string context)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        if (_settings.LogFileType == "JSON")
            logger.AddWriter(new JsonFileWriter(path, context));
        else
            logger.AddWriter(new XmlFileWriter(path, context));
    }

    /// <summary>
    /// Adds daily log writers to the shared execution logger according to LogStorageMode:
    ///   Local  → writes only to DailyLogPath on this machine
    ///   Docker → writes only to DockerLogPath (UNC share on the Docker container)
    ///   Both   → writes to both paths simultaneously
    ///
    /// The machine name is included in each log entry so entries from different
    /// machines can be distinguished in the single shared daily file.
    /// </summary>
    private void AddDailyLogWriters(Logger logger, string context)
    {
        bool writeLocal  = _settings.LogStorageMode is LogStorageMode.Local  or LogStorageMode.Both;
        bool writeDocker = _settings.LogStorageMode is LogStorageMode.Docker or LogStorageMode.Both;

        // Local: write directly to a file on this machine
        if (writeLocal && !string.IsNullOrWhiteSpace(_settings.DailyLogPath))
            AddFileWriter(logger, _settings.DailyLogPath, context);

        // Docker: write directly to the mapped volume folder (same as local)
        if (writeDocker && !string.IsNullOrWhiteSpace(_settings.DockerLogPath))
            AddFileWriter(logger, _settings.DockerLogPath, context);
    }

    // ── Repository methods ────────────────────────────────────────────────────

    public void CreateBackup(BackupCreateRequest backupCreateRequest) => _backupRepository.CreateBackup(backupCreateRequest);
    public void RemoveBackup(Backup backup)                           => _backupRepository.RemoveBackup(backup.Id);
    public Backup? GetBackupById(int backupId)                        => _backupRepository.GetBackupById(backupId);
    public List<Backup> GetBackups(int pageIndex, int pageSize)       => _backupRepository.GetAllBackups();
    public void UpdateBackup(Backup backup)                           => _backupRepository.UpdateBackup(backup);

    public PagedResult<Backup> GetBackupsPage(int pageIndex, int pageSize)
        => _backupRepository.GetBackupsPage(pageIndex, pageSize);

    public PagedResult<Backup> SearchBackupsPage(string? query, int pageIndex, int pageSize)
        => _backupRepository.SearchBackupsPage(query, pageIndex, pageSize);

    public bool ExecuteFromFlag(string flag)
    {
        if (string.IsNullOrWhiteSpace(flag))
            throw new ArgumentException("Flag cannot be empty.");

        flag = flag.Trim().ToLower();

        List<Backup> backupsToExecute = new List<Backup>();
        List<Backup> allBackups       = _backupRepository.GetAllBackups();

        if (flag == "all")
        {
            backupsToExecute = allBackups;
        }
        else if (Regex.IsMatch(flag, @"^\d+$"))
        {
            int id      = int.Parse(flag);
            Backup? b   = _backupRepository.GetBackupById(id) ?? throw new ArgumentException($"Backup with id {id} not found.");
            backupsToExecute.Add(b);
        }
        else if (Regex.IsMatch(flag, @"^\d+\s*-\s*\d+$"))
        {
            string[] parts = flag.Split('-');
            int start = int.Parse(parts[0].Trim());
            int end   = int.Parse(parts[1].Trim());
            if (start > end) throw new ArgumentException("Invalid range: start must be <= end.");
            backupsToExecute = allBackups.Where(b => b.Id >= start && b.Id <= end).ToList();
        }
        else if (Regex.IsMatch(flag, @"^(\d+\s*;\s*)+\d+$"))
        {
            foreach (string part in flag.Split(';'))
            {
                int id    = int.Parse(part.Trim());
                Backup? b = _backupRepository.GetBackupById(id) ?? throw new ArgumentException($"Backup with id {id} not found.");
                backupsToExecute.Add(b);
            }
        }
        else
        {
            throw new ArgumentException("Invalid flag format.");
        }

        if (!backupsToExecute.Any())
            throw new ArgumentException("No backups match the given flag.");

        return ExecuteBackup(backupsToExecute);
    }

    private void LoadSettings()
    {
        try
        {
            string appDataPath  = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsPath = Path.Combine(appDataPath, "EasySave", "settings.json");

            if (File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                _settings   = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            else
            {
                _logger.Log(DictionaryManager.SingleStringToDictionary("File error", "Fichier settings.json introuvable."), LogType.Error);
                _settings = new Settings();
            }
        }
        catch (Exception ex)
        {
            _logger.Log(DictionaryManager.SingleStringToDictionary("Error message", $"Erreur lors du chargement des settings : {ex.Message}"), LogType.Error);
            _settings = new Settings();
        }
    }
}