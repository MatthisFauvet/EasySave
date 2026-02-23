using EasyLog;
using EasyLog.entity;
using EasyLog.utils;
using EasySave.Model;
using EasySave.Repository;
using EasySave.Service;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

// Service responsable de l'exécution des sauvegardes
public class BackupService : IBackupService
{
    // Logger utilisé pour écrire les logs
    private readonly Logger _logger;

    private IBackupRepository _backupRepository;

    // Nom du processus du logiciel métier (ex: "Calculatrice" pour démonstration)
    private readonly string _businessProcessName = "CalculatorApp";

    // Constructeur avec injection du logger
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
    /// Exécute une liste de backups.
    /// Retourne true si tous les backups ont réussi, false sinon.
    /// </summary>
    public bool ExecuteBackup(List<Backup> backups)
    {
        // Indique si l'exécution globale est un succès
        bool isSuccessful = true;
        // Liste des IDs des backups ayant échoué
        List<int> unvalidBackUps = new List<int>();

        // Initialisation du logger global pour l'exécution des backups
        Logger executionLogger = new Logger();
        executionLogger.InitWriters("logs", "Execution of backups");
        executionLogger.Log(
            DictionaryManager.SingleStringToDictionary(
                "message",
                "Starting execution of backups."
            ),
            LogType.Info
        );

        // Parcours de chaque backup à exécuter
        foreach (Backup backup in backups)
        {
            try
            {
                // Exécution d'un backup individuel
                ExecuteSingleBackup(backup);

                // Log de succès
                executionLogger.Log(
                    DictionaryManager.SingleStringToDictionary(
                        "message",
                        $"Backup (ID : {backup.Id}) completed successfully."
                    ),
                    LogType.Info
                );
            }
            catch (Exception ex)
            {
                // Log d'erreur en cas d'échec
                executionLogger.Log(
                    DictionaryManager.SingleStringToDictionary(
                        "message",
                        $"Backup (ID : {backup.Id}) failed : {ex.Message}"
                    ),
                    LogType.Error
                );

                // Ajout de l'ID du backup échoué
                unvalidBackUps.Add(backup.Id);
                // Marque l'exécution globale comme échouée
                isSuccessful = false;
            }
        }

        // Si au moins un backup a échoué, on log la liste des backups concernés
        if (!isSuccessful)
        {
            executionLogger.Log(
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"The following backup(s) failed to execute: {string.Join(", ", unvalidBackUps)}"
                ),
                LogType.Error
            );
        }

        // Fin de l'exécution globale
        executionLogger.Log(
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

    /// <summary>
    /// Exécute un backup individuel :
    /// copie récursivement tous les fichiers du dossier source vers le dossier destination.
    /// </summary>
    private void ExecuteSingleBackup(Backup backup)
    {
        // Initialisation du logger spécifique à ce backup
        Logger backupLogger = new Logger();
        backupLogger.InitWriters(
            backup.DestinationFilePath,
            $"Execution du backup {backup.Id}"
        );

        backupLogger.Log(
            DictionaryManager.SingleStringToDictionary(
                "message",
                $"Starting backup execution (ID: {backup.Id}, Type: {backup.Type})"
            ),
            LogType.Info
        );

        // Vérification du logiciel métier avant démarrage
        if (IsBusinessSoftwareRunning())
        {
            backupLogger.Log(
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"Backup will be stopped : business software detected ({_businessProcessName})"
                ),
                LogType.Warning
            );
           throw new InvalidOperationException(
                $"Backup cannot be executed : business software detected ({_businessProcessName})"
            );
        }

        // Vérifie que le dossier source existe
        if (!Directory.Exists(backup.SourceFilePath))
        {
            backupLogger.Log(
                DictionaryManager.SingleStringToDictionary(
                    "message",
                    $"Source directory not found : {backup.SourceFilePath}"
                ),
                LogType.Error
            );
            // Stoppe l'exécution du backup
            throw new DirectoryNotFoundException(
                $"Source not found : {backup.SourceFilePath}"
            );
        }

        // Crée le dossier de destination s'il n'existe pas
        Directory.CreateDirectory(backup.DestinationFilePath);

        // Récupère tous les fichiers du dossier source (récursivement)
        DirectoryInfo sourceDirectory = new DirectoryInfo(backup.SourceFilePath);
        FileInfo[] files = sourceDirectory.GetFiles("*", SearchOption.AllDirectories);

        // Parcours de chaque fichier à sauvegarder
        foreach (FileInfo sourceFile in files)
        {
            // Calcul du chemin relatif du fichier
            string relativePath = Path.GetRelativePath(
                backup.SourceFilePath,
                sourceFile.FullName
            );

            // Construction du chemin de destination
            string destinationFilePath = Path.Combine(
                backup.DestinationFilePath,
                relativePath
            );

            // Création du dossier parent du fichier de destination
            Directory.CreateDirectory(
                Path.GetDirectoryName(destinationFilePath)!
            );

            // Démarrage du chronomètre pour mesurer le temps de transfert
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                // Copie du fichier (écrase s'il existe déjà)
                sourceFile.CopyTo(destinationFilePath, true);
                stopwatch.Stop();

                // Log du succès du transfert
                Dictionary<string, string> logs = new Dictionary<string, string>
                {
                    { "sourcePath", sourceFile.FullName },
                    { "destinationPath", destinationFilePath },
                    { "fileSize", sourceFile.Length.ToString() },
                    { "transferTimeMs", stopwatch.ElapsedMilliseconds.ToString() },
                    { "backupName", backup.Name }
                };

                backupLogger.Log(logs);

                // Vérification du logiciel métier après chaque fichier copié
                if (IsBusinessSoftwareRunning())
                {
                    backupLogger.Log(
                        DictionaryManager.SingleStringToDictionary(
                            "message",
                            $"Backup stopped after finishing file {sourceFile.FullName} : business software detected ({_businessProcessName})"
                        ),
                        LogType.Error
                    );
                    return;
                }
            }
            catch
            {
                stopwatch.Stop();
                // Log d'erreur si la copie échoue
                Dictionary<string, string> logs = new Dictionary<string, string>
                {
                    { "sourcePath", sourceFile.FullName },
                    { "fileSize", sourceFile.Length.ToString() },
                    { "transferTimeMs", stopwatch.ElapsedMilliseconds.ToString() },
                    { "backupName", backup.Name }
                };

                backupLogger.Log(logs, LogType.Error);
            }
        }
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
}