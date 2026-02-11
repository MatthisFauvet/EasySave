using EasySave.Model;

namespace EasySave.Service;

public interface IBackupService
{
    
    /// <summary>
    /// This method execute a save method on all created backup
    /// </summary>
    /// <param name="backupIds">List of ids for the backups you want to execute</param>
    /// <returns>Return false if issues happened else true</returns>
    public bool ExecuteBackup(List<Backup> backups);
    
    /// <summary>
    /// This method call the repository to create a new backup in the Json file. 
    /// </summary>
    /// <param name="backupCreateRequest">An object containing Backup information except ID</param>
    public void CreateBackup(BackupCreateRequest backupCreateRequest);
    
    /// <summary>
    /// Call BackupRepository to delete backup in the Json file. This methods does not remove the backup at source or
    /// destination filepath. It only remove the backup in the application.
    /// </summary>
    /// <param name="backup">The backup to delete.</param>
    public void RemoveBackup(Backup backup);
    
    /// <summary>
    /// This methods call the repository and return the matching backup (by id) from Json file
    /// </summary>
    /// <param name="backupId">Id for the searched backup</param>
    /// <returns>Return the matching Backup</returns>
    public Backup GetBackupById(int backupId);
    
    /// <summary>
    /// This methods return a List of `pageSize` elements from the index `pageSize * pageIndex`
    /// </summary>
    /// <param name="pageIndex">The page we want to retrieve all element for</param>
    /// <param name="pageSize">Amount of element per page</param>
    /// <returns>List of backups</returns>
    /// TODO In the future, or we can keep using this return type but we also could consider use a custom page object.
    public List<Backup> GetBackups(int pageIndex, int pageSize);
    
    /// <summary>
    /// This method update an already existant backup from a backup object. This method must not change the backup Id.
    /// All field are replaced by params ones and the Id is used to find backup to modify
    /// </summary>
    /// <param name="backup">Backup we want to modify with modification</param>
    public void UpdateBackup(Backup backup);
    
}