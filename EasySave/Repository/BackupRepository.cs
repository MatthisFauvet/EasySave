using System.Security.Cryptography;
using EasySave.Entity;
using EasySave.view;

namespace EasySave.Repository;

/// <summary>
/// In-memory implementation of the IBackupRepository interface.
/// This class is responsible for managing Backup entities
/// during the application lifecycle, without persistence.
/// </summary>
public class BackupRepository : IBackupRepository
{
    // Internal storage for Backup entities
    private readonly List<Backup> _backups = new();

    private readonly int MIN_ID = 0;
    private readonly int MAX_ID = 10000;

    /// <summary>
    /// Adds a new Backup to the repository.
    /// 
    /// This method ensures that:
    /// - the provided Backup object is not null
    /// - the Backup identifier (Id) is unique within the repository
    /// 
    /// These checks guarantee data consistency and prevent
    /// unexpected behavior in retrieval or update operations.
    /// </summary>
    /// <param name="backup">The Backup object to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided Backup object is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a Backup with the same Id already exists.
    /// </exception>
    public void Add(CreateBackupRequest sourceBackup)
    {
        if (sourceBackup == null)
            throw new ArgumentNullException(nameof(sourceBackup));

        Random rd = new Random();
        int Id = rd.Next(MIN_ID, MAX_ID);

        Backup targetBackup = new Backup(Id, sourceBackup.Name, sourceBackup.SourceFilePath, sourceBackup.DestinationFilePath,
            DateTime.Now, BackupType.Sequential);
        
        if (_backups.Any(b => b.Id == Id))
            throw new InvalidOperationException(
                $"A backup with Id {Id} already exists."
            );

        _backups.Add(targetBackup);
    }

    /// <summary>
    /// Removes a Backup from the repository using its unique identifier.
    /// 
    /// The method first attempts to locate the Backup by its Id.
    /// If no matching Backup is found, the repository remains unchanged.
    /// 
    /// Returning a boolean allows the caller to determine
    /// whether the removal was successful.
    /// </summary>
    /// <param name="id">Unique identifier of the Backup to remove.</param>
    /// <returns>
    /// True if the Backup was found and removed; false otherwise.
    /// </returns>
    public bool Remove(int id)
    {
        var existing = GetById(id);

        if (existing == null)
            return false;

        _backups.Remove(existing);
        return true;
    }

    /// <summary>
    /// Retrieves a Backup by its unique identifier.
    /// 
    /// This method performs a search within the repository
    /// and returns the first Backup matching the provided Id.
    /// 
    /// If no Backup is found, null is returned instead of throwing
    /// an exception, allowing safer consumption by the caller.
    /// </summary>
    /// <param name="id">Unique identifier of the Backup.</param>
    /// <returns>
    /// The matching Backup if found; otherwise null.
    /// </returns>
    public Backup? GetById(int id)
    {
        return _backups.FirstOrDefault(b => b.Id == id);
    }

    /// <summary>
    /// Retrieves all Backups stored in the repository.
    /// 
    /// A new list is returned to prevent external code
    /// from modifying the internal collection directly.
    /// This preserves encapsulation and repository integrity.
    /// </summary>
    /// <returns>
    /// A list containing all Backup entities.
    /// </returns>
    public List<Backup> GetAll()
    {
        return new List<Backup>(_backups);
    }

    /// <summary>
    /// Updates an existing Backup in the repository.
    /// 
    /// The Backup is identified by its Id.
    /// If no Backup with the same Id exists, the update is not performed.
    /// 
    /// This approach ensures that only existing entities
    /// can be modified, preventing accidental insertions.
    /// </summary>
    /// <param name="backup">Backup object containing updated data.</param>
    /// <returns>
    /// True if the Backup was found and updated; false otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the provided Backup object is null.
    /// </exception>
    public bool Update(Backup backup)
    {
        if (backup == null)
            throw new ArgumentNullException(nameof(backup));

        var index = _backups.FindIndex(b => b.Id == backup.Id);

        if (index == -1)
            return false;

        _backups[index] = backup;
        return true;
    }
}
