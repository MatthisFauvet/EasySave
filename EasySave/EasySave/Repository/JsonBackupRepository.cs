using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EasySave.Model;

namespace EasySave.Repository
{
    /// <summary>
    /// JSON-based repository responsible for persisting and retrieving Backup definitions.
    ///
    /// Design goals:
    /// - Keep an in-memory cache for fast access
    /// - Persist changes immediately after each write operation
    /// - Ensure thread-safety within a single application instance
    /// </summary>
    public class JsonBackupRepository : IBackupRepository
    {
        private static readonly string FILE_PATH =
            Path.Combine(AppContext.BaseDirectory, "backups", "backups.json");

        /// <summary>
        /// In-memory cache of backups.
        /// </summary>
        private readonly List<Backup> _backups;

        /// <summary>
        /// Synchronization object for thread-safe access (single process).
        /// </summary>
        private readonly object _sync = new();

        /// <summary>
        /// Factory responsible for generating unique incremental IDs.
        /// </summary>
        private readonly IBackupIdFactory _idFactory;

        public JsonBackupRepository()
        {
            _backups = LoadFromFile();

            int maxId = _backups.Count == 0 ? 0 : _backups.Max(b => b.Id);
            _idFactory = new IncrementalBackupIdFactory(maxId);
        }

        public void CreateBackup(BackupCreateRequest backup)
        {
            if (backup == null)
                throw new ArgumentNullException(nameof(backup));

            lock (_sync)
            {
                int newId = _idFactory.NextId();

                // Defensive check (useful if the JSON file was edited manually)
                if (_backups.Any(b => b.Id == newId))
                    throw new InvalidOperationException($"ID collision detected: {newId}");

                var targetBackup = new Backup(
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
            catch (KeyNotFoundException)
            {
                Console.WriteLine("No backup found with Id {0}.", id);
                return false;
            }
        }

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

        public List<Backup> GetAllBackups()
        {
            lock (_sync)
            {
                return new List<Backup>(_backups);
            }
        }

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
        /// Returns a paginated subset of backups.
        ///
        /// - pageIndex is 1-based (1 = first page)
        /// - pageSize defines number of items per page
        /// </summary>
        public PagedResult<Backup> GetBackupsPage(int pageIndex, int pageSize)
        {
            if (pageIndex < 1)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            lock (_sync)
            {
                int total = _backups.Count;

                var items = _backups
                    .OrderBy(b => b.Id) // Stable ordering ensures consistent pagination
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PagedResult<Backup>
                {
                    Items = items,
                    TotalCount = total,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };
            }
        }

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
                return new List<Backup>();
            }
        }

        /// <summary>
        /// Saves the full backup list to disk.
        /// Uses a temporary file to reduce corruption risk.
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
            };
    }
}
