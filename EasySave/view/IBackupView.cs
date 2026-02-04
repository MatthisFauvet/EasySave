namespace EasySave.view;

public interface IBackupView
{

    /// <summary>
    /// This method display actions that users can do :
    ///     0. Create backup;
    ///     1. List Backup;
    ///     2. Remove Backup;
    ///     3. Run Backup;
    /// </summary>
    /// <returns>Id of the selected menu</returns>
    public int ShowMenu();

    /// <summary>
    /// This method display all created backup.
    /// </summary>
    public void ShowBackup();
}