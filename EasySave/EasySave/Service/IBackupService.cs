using EasySave.Model;
using EasySave.Repository;

namespace EasySave.Service;

public interface IBackupService
{
    // Limite de débit en Ko/s pour les transferts (0 = illimité)
    int MaxBandwidthKbps { get; set; }

    // Dossier des logs (chemin relatif à l'AppBase ou absolu pour Docker)
    string LogsDirectory { get; set; }

    // Format du fichier de log ("JSON" ou "XML")
    string LogFileType { get; set; }

    // Lance les backups en parallèle → true si tous réussissent
    Task<bool> ExecuteBackupAsync(List<Backup> backups, Action<Backup>? onBackupUpdate = null);

    // Lance les backups séquentiellement → true si tous réussissent
    public bool ExecuteBackup(List<Backup> backups);

    // Persiste un nouveau backup
    public void CreateBackup(BackupCreateRequest backupCreateRequest);

    // Supprime un backup de l'application (ne touche pas les fichiers)
    public void RemoveBackup(Backup backup);

    // Retourne le backup correspondant à l'id
    public Backup GetBackupById(int backupId);

    // Retourne une page de backups
    public List<Backup> GetBackups(int pageIndex, int pageSize);

    // Met à jour un backup existant (l'id est conservé)
    public void UpdateBackup(Backup backup);

    // Retourne tout l'historique des exécutions
    public List<HistoryEntry> GetHistory();
}
