using System.Diagnostics;
using EasyLog;
using EasyLog.entity;
using EasySave.Entity;
using EasySave.service;

// Service responsable de l'exécution des sauvegardes
public class BackupService : IBackupService
{
    // Logger utilisé pour écrire les logs
    private readonly Logger _logger;

    // Constructeur avec injection du logger
    public BackupService(Logger logger)
    {
        _logger = logger;
    }

    public BackupService()
    {
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
        Logger _logger = new Logger();
        _logger.InitWriters("logs", "Execution of backups");
        _logger.Log("Starting execution of backups.", LogType.Info);

        // Parcours de chaque backup à exécuter
        foreach (Backup backup in backups)
        {
            try
            {
                // Exécution d'un backup individuel
                ExecuteSingleBackup(backup);

                // Log de succès
                _logger.Log($"Backup (ID : {backup}) completed successfully.", LogType.Info);
            }
            catch (Exception ex)
            {
                // Log d'erreur en cas d'échec
                _logger.Log($"Backup (ID : {backup}) failed : {ex.Message}", LogType.Error);

                // Ajout de l'ID du backup échoué
                unvalidBackUps.Add(backup.Id);

                // Marque l'exécution globale comme échouée
                isSuccessful = false;
            }
        }

        // Si au moins un backup a échoué, on log la liste des backups concernés
        if (!isSuccessful)
        {
            _logger.Log(
                $"The following backup(s) failed to execute: {string.Join(", ", unvalidBackUps)}",
                LogType.Error
            );
        }

        // Fin de l'exécution globale
        _logger.Log("Finished execution of backups.", LogType.Info);

        return isSuccessful;
    }

    /// <summary>
    /// Exécute un backup individuel :
    /// copie récursivement tous les fichiers du dossier source vers le dossier destination.
    /// </summary>
   private void ExecuteSingleBackup(Backup backup)
    {
        // Initialisation du logger spécifique à ce backup
        Logger _logger = new Logger();
        _logger.InitWriters(
            backup.DestinationFilePath,
            $"Execution du backup {backup.Id}"
        );
        _logger.Log(
            $"Starting backup execution (ID: {backup.Id}, Type: {backup.Type})",
            LogType.Info
        );

        // Vérifie que le dossier source existe
        if (!Directory.Exists(backup.SourceFilePath))
        {
            _logger.Log(
                $"Source directory not found : {backup.SourceFilePath}",
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
        var sourceDirectory = new DirectoryInfo(backup.SourceFilePath);
        var files = sourceDirectory.GetFiles("*", SearchOption.AllDirectories);

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
                _logger.Log(
                    $"sourcePath: {sourceFile.FullName}, " +
                    $"targetPath: {destinationFilePath}, " +
                    $"fileSize: {sourceFile.Length}, " +
                    $"transferTimeMs: {stopwatch.ElapsedMilliseconds}, " +
                    $"backupName: {backup.Name}",
                    LogType.Info
                );
            }
            catch
            {
                stopwatch.Stop();

                // Log d'erreur si la copie échoue
                _logger.Log(
                    $"sourcePath: {sourceFile.FullName}, " +
                    $"fileSize: {sourceFile.Length}, " +
                    $"transferTimeMs: {-1}, " +
                    $"backupName: {backup.Name}",
                    LogType.Error
                );
            }
        }
   }
}
