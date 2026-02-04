using EasyLog.writers;

namespace EasySave.service;

public interface IBackupService
{
    
    /// <summary>
    /// This method execute a save method on all created backup
    /// </summary>
    /// <param name="backupIds">List of ids for the backups you want to execute</param>
    /// <returns>Return false if issues happened else true</returns>
    public bool ExecuteBackup(List<int> backupIds);
}
