using System.IO;
using System.Text.Json;
using EasySave.Model;

namespace EasySave.Repository;

public class JsonBackupRepository : IBackupRepository
{
    // JSON file path used as persistent storage
    private static readonly string FILE_PATH =
        Path.Combine(
            AppContext.BaseDirectory,
            "backups",
            "backups.json"
        );

    // In-memory cache loaded from the JSON file
    private readonly List<Backup> _backups;

    // Simple lock for thread-safety (list + file access)
    private readonly object _sync = new();
    
    private readonly int MAX_ID = 4;
    
    private readonly int MIN_ID = 0;
    
    public JsonBackupRepository()
    {
        _backups = LoadFromFile();
    }

    /// <summary>
    /// Adds a new Backup and immediately persists the updated list to the JSON file.
    /// 
    /// Constraints:
    /// - backup must not be null
    /// - backup.Id must be unique
    /// 
    /// Persistence strategy:
    /// - The repository keeps an in-memory list
    /// - After each change, the full list is serialized and saved to disk
    /// </summary>
    public void CreateBackup(BackupCreateRequest backup)
    {
        if (backup == null)
            throw new ArgumentNullException(nameof(backup));

        lock (_sync)
        {
            // Limite de 5 backups
            if (_backups.Count >= 5)
                throw new InvalidOperationException("Impossible d'ajouter plus de 5 backups.");

            // Cherche le plus petit ID libre entre 1 et 5
            int newId = Enumerable.Range(1, 5)
                .First(id => !_backups.Any(b => b.Id == id));

            Backup targetBackup = new Backup(
                newId,
                backup.Name,
                backup.SourceFilePath,
                backup.DestinationFilePath,
                DateTime.Now,
                backup.Type
            );

            _backups.Add(targetBackup);
            SaveToFile(_backups);
        }
    }


    /// <summary>
    /// Removes a Backup by its Id and persists the change.
    /// 
    /// UML-aligned behavior (void):
    /// - If the Id does not exist, an exception is thrown
    ///   because "Remove(id)" implies the target must exist.
    /// </summary>
    public bool RemoveBackup(int id)
    {
        try
        {
            lock (_sync)
            {
                var existing = _backups.FirstOrDefault(b => b.Id == id);
                if (existing == null)
                    throw new KeyNotFoundException($"No backup found with Id {id}.");

                _backups.Remove(existing);
                SaveToFile(_backups);
                return true;
            }
        }
        catch (KeyNotFoundException e)
        {
            Console.WriteLine("No backup found with Id {0}.", id);
            return false;
        }
        
    }
    
    /// <summary>
    /// Retrieves a Backup by its Id.
    /// 
    /// UML-aligned behavior (non-null return):
    /// - If the Id does not exist, an exception is thrown.
    /// 
    /// Note:
    /// - No disk read is performed here (the repository uses an in-memory cache).
    /// </summary>
    public Backup? GetBackupById(int id)
    {
        lock (_sync)
        {
            var backup = _backups.FirstOrDefault(b => b.Id == id);
            if (backup == null)
                throw new KeyNotFoundException($"No backup found with Id {id}.");

            return backup;
        }
    }

    /// <summary>
    /// Returns all backups currently stored.
    /// 
    /// A copy is returned to avoid exposing the internal list reference.
    /// </summary>
    public List<Backup> GetAllBackups()
    {
        lock (_sync)
        {
            return new List<Backup>(_backups);
        }
    }

    /// <summary>
    /// Updates an existing Backup (matched by Id) and persists the change.
    /// 
    /// UML-aligned behavior (void):
    /// - If the Id does not exist, an exception is thrown.
    /// 
    /// Update strategy:
    /// - Full replacement of the entity in the list.
    /// </summary>
    public bool UpdateBackup(Backup backup)
    {
        if (backup == null)
            throw new ArgumentNullException(nameof(backup));

        lock (_sync)
        {
            var index = _backups.FindIndex(b => b.Id == backup.Id);
            if (index == -1)
                throw new KeyNotFoundException($"No backup found with Id {backup.Id}.");

            _backups[index] = backup;
            SaveToFile(_backups);
            return true;
        }
    }

    /// <summary>
    /// Loads backups from the JSON file.
    /// 
    /// Behavior:
    /// - If the file does not exist: returns an empty list
    /// - If the file is invalid/corrupted: returns an empty list (safe fallback)
    /// </summary>
    private List<Backup> LoadFromFile()
    {
        try
        {
            if (!File.Exists(FILE_PATH))
                return new List<Backup>();

            var json = File.ReadAllText(FILE_PATH);
            if (string.IsNullOrWhiteSpace(json))
                return new List<Backup>();

            return JsonSerializer.Deserialize<List<Backup>>(json, GetJsonOptions())
                   ?? new List<Backup>();
        }
        catch
        {
            // If JSON is corrupted, we start fresh.
            return new List<Backup>();
        }
    }

    /// <summary>
    /// Saves the full list of backups into the JSON file (atomic write).
    /// 
    /// Atomic write approach:
    /// - Write to a temporary file
    /// - Replace the original file
    /// 
    /// This prevents partial/corrupted JSON if the application stops during writing.
    /// </summary>
    private void SaveToFile(List<Backup> backups)
    {
        var directory = Path.GetDirectoryName(FILE_PATH);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(backups, GetJsonOptions());

        var tempPath = FILE_PATH + ".tmp";
        File.WriteAllText(tempPath, json);

        if (File.Exists(FILE_PATH))
            File.Delete(FILE_PATH);

        File.Move(tempPath, FILE_PATH);
    }

    private static JsonSerializerOptions GetJsonOptions() =>
        new JsonSerializerOptions
        {
            WriteIndented = true
            // DateTime is handled natively (ISO 8601) by System.Text.Json.
        };
}
