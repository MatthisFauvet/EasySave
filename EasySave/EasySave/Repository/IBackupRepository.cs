using EasySave.Model;

namespace EasySave.Repository;

/// <summary>
/// Defines the contract for managing Backup entities.
/// 
/// This interface abstracts the persistence logic
/// and allows the application to remain independent
/// from the concrete storage implementation.
/// </summary>
public interface IBackupRepository
{
    /// <summary>
    /// Adds a new Backup to the repository.
    /// 
    /// Implementations must ensure the uniqueness
    /// of the Backup identifier (Id).
    /// </summary>
    /// <param name="backup">Backup entity to add.</param>
    void CreateBackup(BackupCreateRequest backup);

    /// <summary>
    /// Removes a Backup using its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the Backup.</param>
    /// <returns>
    /// True if the Backup was removed; false otherwise.
    /// </returns>
    bool RemoveBackup(int id);

    /// <summary>
    /// Retrieves a Backup by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the Backup.</param>
    /// <returns>
    /// The Backup if found; otherwise null.
    /// </returns>
    Backup? GetBackupById(int id);

    /// <summary>
    /// Retrieves all Backups managed by the repository.
    /// </summary>
    /// <returns>
    /// A list containing all Backup entities.
    /// </returns>
    List<Backup> GetAllBackups();

    /// <summary>
    /// Updates an existing Backup.
    /// 
    /// The Backup is identified by its Id.
    /// If no matching Backup exists, the update fails.
    /// </summary>
    /// <param name="backup">Backup entity with updated values.</param>
    /// <returns>
    /// True if the Backup was updated; false otherwise.
    /// </returns>
    bool UpdateBackup(Backup backup);


    /// <summary>
    /// Returns a paginated list of backups.
    ///
    /// - pageIndex is 1-based (1 = first page).
    /// - pageSize defines the number of items per page.
    /// 
    /// This method helps limit the amount of data sent to the UI
    /// when the number of backups becomes large.
    /// </summary>
    /// <param name="pageIndex">The requested page index (1-based).</param>
    /// <param name="pageSize">Number of backups per page.</param>
    /// <returns>A paginated result containing the backups for the page and pagination metadata.</returns>
    PagedResult<Backup> GetBackupsPage(int pageIndex, int pageSize);

}