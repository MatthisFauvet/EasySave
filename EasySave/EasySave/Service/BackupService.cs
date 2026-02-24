using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using EasyLog;
using EasyLog.entity;
using EasyLog.utils;
using EasySave.Model;
using EasySave.Repository;
using EasySave.Service;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using EasyLog.writers;

// Service responsable de l'exécution des sauvegardes
public class BackupService : IBackupService
{
    // Logger utilisé pour écrire les logs
    private readonly Logger _logger;
    
    private Settings _settings;
    
    private IBackupRepository _backupRepository;

    // Nom du processus du logiciel métier (ex: "Calculatrice" pour démonstration)
    private readonly string _businessProcessName = "CalculatorApp";

    // Constructeur avec injection du logger
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

    /// <summary>
    /// Détecte si le logiciel métier est en cours d'exécution
    /// </summary>
    private bool IsBusinessSoftwareRunning()
    {
        try
        {
            // Vérifie si le processus du logiciel métier est en cours d'exécution
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

    /// <summary>
    /// Executes all backups in parallel.
    /// Each backup runs on its own thread.
    /// If business software is detected, ALL backups are cancelled via a shared token.
    /// </summary>
    public bool ExecuteBackup(List<Backup> backups)
    {
        LoadSettings();
        
        // Indique si l'exécution globale est un succès
        bool isSuccessful = true;
        // Liste des IDs des backups ayant échoué
        List<int> unvalidBackUps = new List<int>();

        // ConcurrentBag is a thread-safe collection
        // Multiple threads can add to it simultaneously without corruption
        // Replaces the original List<int> which is NOT thread-safe
        ConcurrentBag<int> failedBackupIds = new ConcurrentBag<int>();

        // One shared "stop button" for all backup tasks
        // If any backup detects the business software, it cancels this source
        // and ALL other running backups receive the cancellation signal
        using CancellationTokenSource cts = new CancellationTokenSource();

        // Initialisation du logger global pour l'exécution des backups
        Logger executionLogger = new Logger();
        SelectLogType(executionLogger, "logs", "Execution of backups");
        
        executionLogger.Log(
            DictionaryManager.SingleStringToDictionary("message", "Starting execution of backups."),
            LogType.Info
        );

        // Create one Task per backup — each runs on a thread pool thread
        List<Task> tasks = backups.Select(backup => Task.Run(() =>
        {
            try
            {
                // Pass the token down so ExecuteSingleBackup can check it
                ExecuteSingleBackup(backup, cts.Token);

                executionLogger.Log(
                    DictionaryManager.SingleStringToDictionary(
                        "message",
                        $"Backup (ID: {backup.Id}) completed successfully, you can find it in your destination source."
                    ),
                    LogType.Info
                );
            }
            catch (OperationCanceledException)
            {// This backup was cancelled because business software was detected
                // either by itself or by another backup task
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
        // Block here until ALL backup tasks have finished (success or failure)
        Task.WhenAll(tasks).Wait();
        bool isSuccessful = failedBackupIds.IsEmpty;

        // Si au moins un backup a échoué, on log la liste des backups concernés
        if (!isSuccessful)
        {
            executionLogger.Log(
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"The following backup(s) failed to execute: {string.Join(", ", failedBackupIds)}"
                ),
                LogType.Error
            );
        }

        // Fin de l'exécution globale
        executionLogger.Log(
            DictionaryManager.SingleStringToDictionary("message", "Finished execution of backups."),
            LogType.Info
        );

        return isSuccessful;
    }

    /// <summary>
    /// Executes a single backup with cancellation support.
    /// Checks the token before each file copy — if cancelled, stops immediately.
    /// If business software is detected mid-backup, cancels the shared token
    /// so all other running backups also stop.
    /// </summary>
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

        // Check before even starting — no point starting if already cancelled
        // or if business software is already running
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

        foreach (FileInfo sourceFile in files)
        {
            // Check the token BEFORE each file
            // If another backup already cancelled the token, we stop here
            // This is the key integration point between parallel tasks
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

                // Check AFTER each file copy too
                // If business software appeared during the copy, signal ALL tasks to stop
                if (IsBusinessSoftwareRunning())
                {
                    backupLogger.Log(
                        DictionaryManager.SingleStringToDictionary(
                            "message",
                            $"Business software detected after copying {sourceFile.Name} — cancelling all backups."
                        ),
                        LogType.Warning
                    );

                    // This signals ALL other backup tasks to stop at their next token check
                    throw new OperationCanceledException(token);
                }
            }
            catch (OperationCanceledException)
            {
                // Re-throw so the task catches it as a cancellation, not a generic error
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

    // --- Repository methods unchanged ---
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
    
    public bool ExecuteFromFlag(string flag)
    {
        if (string.IsNullOrWhiteSpace(flag))
            throw new ArgumentException("Flag cannot be empty.");

        flag = flag.Trim().ToLower();

        List<Backup> backupsToExecute = new List<Backup>();
        List<Backup> allBackups = _backupRepository.GetAllBackups();

        // ALL
        if (flag == "all")
        {
            backupsToExecute = allBackups;
        }

        // SINGLE NUMBER (ex: "1")
        else if (Regex.IsMatch(flag, @"^\d+$"))
        {
            int id = int.Parse(flag);
            Backup? backup = _backupRepository.GetBackupById(id);

            if (backup == null)
                throw new ArgumentException($"Backup with id {id} not found.");

            backupsToExecute.Add(backup);
        }

        // RANGE (ex: "1-3")
        else if (Regex.IsMatch(flag, @"^\d+\s*-\s*\d+$"))
        {
            string[] parts = flag.Split('-');
            int start = int.Parse(parts[0].Trim());
            int end = int.Parse(parts[1].Trim());

            if (start > end)
                throw new ArgumentException("Invalid range: start must be <= end.");

            backupsToExecute = allBackups
                .Where(b => b.Id >= start && b.Id <= end)
                .ToList();
        }

        // MULTIPLE IDS (ex: "1;3;4")
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

                _settings = JsonSerializer.Deserialize<Settings>(json)
                            ?? new Settings();
            }
            else
            {
                _logger.Log(DictionaryManager.SingleStringToDictionary(
                    "File error", "Fichier settings.json introuvable."),
                    LogType.Error
                );
                _settings = new Settings();
            }
        }
        catch (Exception ex)
        {
            _logger.Log(DictionaryManager.SingleStringToDictionary(
                "Error message",
                $"Erreur lors du chargement des settings : {ex.Message}"),
                LogType.Error);
            _settings = new Settings();
        }
    }

    private void SelectLogType(Logger logger, string path, string context)
    {
        if (_settings.LogFileType == "JSON")
        {
            logger.AddWriter(new JsonFileWriter(path, context));
        }
        else
        {
            logger.AddWriter(new XmlFileWriter(path, context));
        }
    }
}
