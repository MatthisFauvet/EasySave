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

// Service responsable de l'exécution des sauvegardes
public class BackupService : IBackupService
{
    private readonly Logger _logger;
    private Settings _settings;
    private IBackupRepository _backupRepository;

    private readonly string _businessProcessName = "CalculatorApp";

    public BackupService(Logger logger)
    {
        _logger = logger;
        InitializeBackupRepository();
        LoadSettings();
    }

    public BackupService()
    {
        _logger = new Logger();
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
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"Error checking business software process: {ex.Message}"
                ),
                LogType.Error
            );
            return false;
        }
    }

    public bool ExecuteBackup(List<Backup> backups)
    {
        LoadSettings();

        bool isSuccessful = true;
        ConcurrentBag<int> failedBackupIds = new ConcurrentBag<int>();

        using CancellationTokenSource cts = new CancellationTokenSource();

        Logger executionLogger = new Logger();
        SelectLogType(executionLogger, "logs", "Execution of backups");

        executionLogger.Log(
            DictionaryManager.SingleStringToDictionary("message", "Starting execution of backups."),
            LogType.Info
        );

        List<Task> tasks = backups.Select(backup => Task.Run(() =>
        {
            try
            {
                ExecuteSingleBackup(backup, cts.Token);

                executionLogger.Log(
                    DictionaryManager.SingleStringToDictionary(
                        "message",
                        $"Backup (ID: {backup.Id}) completed successfully."
                    ),
                    LogType.Info
                );
            }
            catch (OperationCanceledException)
            {
                executionLogger.Log(
                    DictionaryManager.SingleStringToDictionary(
                        "message",
                        $"Backup (ID: {backup.Id}) was cancelled — business software detected."
                    ),
                    LogType.Warning
                );
                failedBackupIds.Add(backup.Id);
            }
            catch (Exception ex)
            {
                executionLogger.Log(
                    DictionaryManager.SingleStringToDictionary(
                        "message",
                        $"Backup (ID: {backup.Id}) failed: {ex.Message}"
                    ),
                    LogType.Error
                );
                failedBackupIds.Add(backup.Id);
            }
        }, cts.Token)).ToList();

        Task.WhenAll(tasks).Wait();

        isSuccessful = failedBackupIds.IsEmpty;

        if (!isSuccessful)
        {
            executionLogger.Log(
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"The following backup(s) failed: {string.Join(", ", failedBackupIds)}"
                ),
                LogType.Error
            );
        }

        executionLogger.Log(
            DictionaryManager.SingleStringToDictionary("message", "Finished execution of backups."),
            LogType.Info
        );

        return isSuccessful;
    }

    private void ExecuteSingleBackup(Backup backup, CancellationToken token)
    {
        Logger backupLogger = new Logger();
        SelectLogType(backupLogger,
            backup.DestinationFilePath,
            $"Execution du backup {backup.Id}");

        backupLogger.Log(
            DictionaryManager.SingleStringToDictionary(
                "message",
                $"Starting backup execution (ID: {backup.Id}, Type: {backup.Type})"
            ),
            LogType.Info
        );

        token.ThrowIfCancellationRequested();

        if (IsBusinessSoftwareRunning())
        {
            backupLogger.Log(
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"Backup blocked before start — business software detected ({_businessProcessName})"
                ),
                LogType.Warning
            );
            throw new InvalidOperationException(
                $"Backup cannot start — business software detected ({_businessProcessName})"
            );
        }

        if (!Directory.Exists(backup.SourceFilePath))
        {
            backupLogger.Log(
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"Source directory not found: {backup.SourceFilePath}"
                ),
                LogType.Error
            );
            throw new DirectoryNotFoundException($"Source not found: {backup.SourceFilePath}");
        }

        Directory.CreateDirectory(backup.DestinationFilePath);

        DirectoryInfo sourceDirectory = new DirectoryInfo(backup.SourceFilePath);
        FileInfo[] files = sourceDirectory.GetFiles("*", SearchOption.AllDirectories);

        // ===========================
        // PRIORITY EXTENSIONS LOGIC
        // ===========================
        if (_settings?.PriorityExtensions != null && _settings.PriorityExtensions.Any())
        {
            HashSet<string> priorityExtensions = new HashSet<string>(
                _settings.PriorityExtensions
                    .Where(ext => !string.IsNullOrWhiteSpace(ext))
                    .Select(ext => ext.ToLowerInvariant())
            );

            files = files
                .OrderByDescending(f => priorityExtensions.Contains(f.Extension.ToLowerInvariant()))
                .ToArray();
        }
        // ===========================

        foreach (FileInfo sourceFile in files)
        {
            token.ThrowIfCancellationRequested();

            string relativePath = Path.GetRelativePath(backup.SourceFilePath, sourceFile.FullName);
            string destinationFilePath = Path.Combine(backup.DestinationFilePath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath)!);

            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                sourceFile.CopyTo(destinationFilePath, true);
                stopwatch.Stop();

                Dictionary<string, string> logs = new Dictionary<string, string>
                {
                    { "sourcePath", sourceFile.FullName },
                    { "destinationPath", destinationFilePath },
                    { "fileSize", sourceFile.Length.ToString() },
                    { "transferTimeMs", stopwatch.ElapsedMilliseconds.ToString() },
                    { "backupName", backup.Name },
                };

                backupLogger.Log(logs);

                if (IsBusinessSoftwareRunning())
                {
                    backupLogger.Log(
                        DictionaryManager.SingleStringToDictionary(
                            "message",
                            $"Business software detected after copying {sourceFile.Name} — cancelling all backups."
                        ),
                        LogType.Warning
                    );

                    throw new OperationCanceledException(token);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                stopwatch.Stop();
                Dictionary<string, string> logs = new Dictionary<string, string>
                {
                    { "sourcePath", sourceFile.FullName },
                    { "fileSize", sourceFile.Length.ToString() },
                    { "transferTimeMs", stopwatch.ElapsedMilliseconds.ToString() },
                    { "backupName", backup.Name },
                };
                backupLogger.Log(logs, LogType.Error);
            }
        }
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

    public PagedResult<Backup> GetBackupsPage(int pageIndex, int pageSize)
    {
        return _backupRepository.GetBackupsPage(pageIndex, pageSize);
    }

    public PagedResult<Backup> SearchBackupsPage(string? query, int pageIndex, int pageSize)
        => _backupRepository.SearchBackupsPage(query, pageIndex, pageSize);

    public void UpdateBackup(Backup backup)
    {
        _backupRepository.UpdateBackup(backup);
    }

    public bool ExecuteFromFlag(string flag)
    {
        if (string.IsNullOrWhiteSpace(flag))
            throw new ArgumentException("Flag cannot be empty.");

        flag = flag.Trim().ToLower();

        List<Backup> backupsToExecute = new List<Backup>();
        List<Backup> allBackups = _backupRepository.GetAllBackups();

        if (flag == "all")
        {
            backupsToExecute = allBackups;
        }
        else if (Regex.IsMatch(flag, @"^\d+$"))
        {
            int id = int.Parse(flag);
            Backup? backup = _backupRepository.GetBackupById(id);

            if (backup == null)
                throw new ArgumentException($"Backup with id {id} not found.");

            backupsToExecute.Add(backup);
        }
        else if (Regex.IsMatch(flag, @"^\d+\s*-\s*\d+$"))
        {
            string[] parts = flag.Split('-');
            int start = int.Parse(parts[0].Trim());
            int end = int.Parse(parts[1].Trim());

            if (start > end)
                throw new ArgumentException("Invalid range.");

            backupsToExecute = allBackups
                .Where(b => b.Id >= start && b.Id <= end)
                .ToList();
        }
        else if (Regex.IsMatch(flag, @"^(\d+\s*;\s*)+\d+$"))
        {
            string[] parts = flag.Split(';');

            foreach (string part in parts)
            {
                int id = int.Parse(part.Trim());
                Backup? backup = _backupRepository.GetBackupById(id);

                if (backup == null)
                    throw new ArgumentException($"Backup with id {id} not found.");

                backupsToExecute.Add(backup);
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
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsPath = Path.Combine(appDataPath, "EasySave", "settings.json");

            if (File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                _settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            else
            {
                _settings = new Settings();
            }
        }
        catch
        {
            _settings = new Settings();
        }
    }

    private void SelectLogType(Logger logger, string path, string context)
    {
        if (_settings.LogFileType == "JSON")
            logger.AddWriter(new JsonFileWriter(path, context));
        else
            logger.AddWriter(new XmlFileWriter(path, context));
    }
}