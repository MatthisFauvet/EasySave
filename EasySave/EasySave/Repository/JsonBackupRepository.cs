using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using EasySave.Model;

namespace EasySave.Repository
{
    /// <summary>
    /// JSON-based repository responsible for persisting and retrieving <see cref="Backup"/> definitions.
    ///
    /// Design goals:
    /// - Keep an in-memory cache for fast reads.
    /// - Persist changes immediately after each write operation.
    /// - Provide thread-safe access (concurrent reads, exclusive writes).
    /// - Ensure safe file persistence using an atomic write strategy.
    /// - Prepare for multi-process scenarios by using a named OS mutex during file writes.
    /// </summary>
    public class JsonBackupRepository : IBackupRepository
    {
        /// <summary>
        /// Persistent storage location for backups.
        /// The path is resolved under the application's base directory.
        /// </summary>
        private static readonly string FILE_PATH =
            Path.Combine(AppContext.BaseDirectory, "backups", "backups.json");

        /// <summary>
        /// In-memory cache of backups loaded from <see cref="FILE_PATH"/>.
        /// </summary>
        private readonly List<Backup> _backups;

        /// <summary>
        /// Reader/writer lock:
        /// - Allows multiple concurrent readers (GetAll/GetById/Page).
        /// - Ensures exclusive access for writers (Create/Update/Remove).
        /// </summary>
        private readonly ReaderWriterLockSlim _rwLock = new(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// ID factory responsible for issuing unique backup IDs.
        /// Uses a monotonic strategy (maxId + 1) to avoid collisions under concurrency.
        /// </summary>
        private readonly IBackupIdFactory _idFactory;

        /// <summary>
        /// Named mutex to protect file writes across multiple processes.
        /// This prevents two application instances from writing the JSON file at the same time.
        /// </summary>
        private static readonly Mutex _fileMutex =
            new Mutex(false, @"Global\EasySave_Backups_Json_Lock");

        /// <summary>
        /// Initializes the repository by loading the JSON file into memory
        /// and seeding the ID factory with the current maximum ID.
        /// </summary>
        public JsonBackupRepository()
        {
            _backups = LoadFromFile();

            int maxId = _backups.Count == 0 ? 0 : _backups.Max(b => b.Id);
            _idFactory = new IncrementalBackupIdFactory(maxId);
        }

        /// <summary>
        /// Creates a new backup definition and persists the updated list to disk.
        ///
        /// Thread-safety:
        /// - Generates the ID through an ID factory.
        /// - Updates the in-memory list under a write lock.
        /// - Persists a snapshot to disk to keep file I/O out of the critical section.
        ///
        /// Notes:
        /// - A defensive collision check is performed (useful if the JSON file is modified externally).
        /// </summary>
        public void CreateBackup(BackupCreateRequest backup)
        {
            if (backup == null)
                throw new ArgumentNullException(nameof(backup));

            int newId = _idFactory.NextId();

            var targetBackup = new Backup(
                newId,
                backup.Name,
                backup.SourceFilePath,
                backup.DestinationFilePath,
                DateTime.Now,
                backup.Type
            );

            List<Backup> snapshot;

            _rwLock.EnterWriteLock();
            try
            {
                // Defensive check: helps detect unexpected collisions (e.g., external file edits).
                if (_backups.Any(b => b.Id == newId))
                    throw new InvalidOperationException($"ID collision detected: {newId}");

                _backups.Add(targetBackup);

                // Snapshot prevents exposing internal list and allows persisting outside the write lock.
                snapshot = _backups.ToList();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            SaveToFile(snapshot);
        }

        /// <summary>
        /// Removes a backup definition by ID and persists the change.
        ///
        /// Returns:
        /// - true if the backup was removed
        /// - false if the backup does not exist (error is logged)
        /// </summary>
        public bool RemoveBackup(int id)
        {
            try
            {
                List<Backup> snapshot;

                _rwLock.EnterWriteLock();
                try
                {
                    var existing = _backups.FirstOrDefault(b => b.Id == id);
                    if (existing == null)
                        throw new KeyNotFoundException($"No backup found with Id {id}.");

                    _backups.Remove(existing);
                    snapshot = _backups.ToList();
                }
                finally
                {
                    _rwLock.ExitWriteLock();
                }

                SaveToFile(snapshot);
                return true;
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("No backup found with Id {0}.", id);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a backup by ID from the in-memory cache.
        ///
        /// Thread-safety:
        /// - Read lock ensures a consistent view during concurrent writes.
        ///
        /// Throws:
        /// - <see cref="KeyNotFoundException"/> if the backup does not exist.
        /// </summary>
        public Backup? GetBackupById(int id)
        {
            _rwLock.EnterReadLock();
            try
            {
                var backup = _backups.FirstOrDefault(b => b.Id == id);
                if (backup == null)
                    throw new KeyNotFoundException($"No backup found with Id {id}.");

                return backup;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns all backups from the in-memory cache.
        /// A copy is returned to avoid exposing the internal list reference.
        /// </summary>
        public List<Backup> GetAllBackups()
        {
            _rwLock.EnterReadLock();
            try
            {
                return new List<Backup>(_backups);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Updates an existing backup (matched by ID) and persists the change.
        ///
        /// Throws:
        /// - <see cref="KeyNotFoundException"/> if the backup does not exist.
        /// </summary>
        public bool UpdateBackup(Backup backup)
        {
            if (backup == null)
                throw new ArgumentNullException(nameof(backup));

            List<Backup> snapshot;

            _rwLock.EnterWriteLock();
            try
            {
                var index = _backups.FindIndex(b => b.Id == backup.Id);
                if (index == -1)
                    throw new KeyNotFoundException($"No backup found with Id {backup.Id}.");

                _backups[index] = backup;
                snapshot = _backups.ToList();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            SaveToFile(snapshot);
            return true;
        }

        /// <summary>
        /// Returns a paginated subset of backups.
        ///
        /// Pagination rules:
        /// - pageIndex is 1-based (pageIndex = 1 => first page).
        /// - pageSize defines the number of items per page.
        /// - A stable ordering is applied to ensure consistent pagination.
        ///
        /// Thread-safety:
        /// - Uses a read lock to keep a consistent view while paging.
        /// </summary>
        public PagedResult<Backup> GetBackupsPage(int pageIndex, int pageSize)
        {
            if (pageIndex < 1) throw new ArgumentOutOfRangeException(nameof(pageIndex));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            _rwLock.EnterReadLock();
            try
            {
                int total = _backups.Count;

                var items = _backups
                    .OrderBy(b => b.Id) // Stable ordering for consistent pages
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
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Loads backups from the JSON file.
        ///
        /// Behavior:
        /// - If the file does not exist or is empty: returns an empty list.
        /// - If the JSON is invalid/corrupted: returns an empty list (safe fallback).
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
                // Safe fallback: if JSON is corrupted, the repository starts with an empty cache.
                return new List<Backup>();
            }
        }

        /// <summary>
        /// Persists the full list of backups to disk using an atomic write strategy.
        ///
        /// Multi-process safety:
        /// - A named mutex is used to prevent concurrent writes from different processes.
        ///
        /// Atomic write strategy:
        /// - Serialize to a temporary file
        /// - Replace the destination file in a single atomic operation (when possible)
        /// This avoids leaving a partially written or corrupted JSON file in case of a crash.
        /// </summary>
        private void SaveToFile(List<Backup> backups)
        {
            _fileMutex.WaitOne();
            try
            {
                var directory = Path.GetDirectoryName(FILE_PATH);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                var json = JsonSerializer.Serialize(backups, GetJsonOptions());

                var tempPath = FILE_PATH + ".tmp";
                File.WriteAllText(tempPath, json);

                // Atomic replace is preferred over Delete+Move to prevent a "missing file" window.
                if (File.Exists(FILE_PATH))
                {
                    File.Replace(tempPath, FILE_PATH, null);
                }
                else
                {
                    File.Move(tempPath, FILE_PATH);
                }
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// JSON serializer options used for persistence.
        /// </summary>
        private static JsonSerializerOptions GetJsonOptions() =>
            new JsonSerializerOptions
            {
                WriteIndented = true
            };
    }
}
