using EasyLog.writers;
using EasySave.Entity;

namespace EasySave.service;

public interface IBackupService
{
    
    /// <summary>
    /// This method execute a save method on all created backup
    /// </summary>
    /// <param name="backup">List of backups you want to execute</param>
    /// <returns>Return false if issues happened else true</returns>
    public bool ExecuteBackup(List<Backup> backups);
}
