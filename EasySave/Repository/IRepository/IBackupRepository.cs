using EasySave.Entity;

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
    void Add(Backup backup);

    /// <summary>
    /// Removes a Backup using its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the Backup.</param>
    /// <returns>
    /// True if the Backup was removed; false otherwise.
    /// </returns>
    bool Remove(int id);

    /// <summary>
    /// Retrieves a Backup by its unique identifier.
    /// </summary>
    /// <param name="id">Unique identifier of the Backup.</param>
    /// <returns>
    /// The Backup if found; otherwise null.
    /// </returns>
    Backup? GetById(int id);

    /// <summary>
    /// Retrieves all Backups managed by the repository.
    /// </summary>
    /// <returns>
    /// A list containing all Backup entities.
    /// </returns>
    List<Backup> GetAll();

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
    bool Update(Backup backup);
}
