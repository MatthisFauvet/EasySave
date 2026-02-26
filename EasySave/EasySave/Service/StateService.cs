using System.IO;
using System.Text.Json;
using EasySave.Model;

namespace EasySave.Service;

/// <summary>
/// Manages the state.json file located in %AppData%\EasySave\.
/// 
/// Responsibilities:
/// - Initialize a backup entry before it starts
/// - Update progress after each file is copied
/// - Mark a backup as completed, cancelled, or errored
/// - Provide a thread-safe read/write mechanism for parallel backup tasks
/// </summary>
public class StateService
{
    private readonly string _stateFilePath;
    private readonly object _lock = new object();

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public StateService()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _stateFilePath = Path.Combine(appData, "EasySave", "state.json");
        EnsureDirectoryExists();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Registers a backup job before execution starts.
    /// Sets status to Running, resets progress counters.
    /// </summary>
    public void Initialize(string name, string source, string destination, int totalFiles, long totalSizeBytes)
    {
        var state = new BackupState
        {
            Name              = name,
            Source            = source,
            Destination       = destination,
            TotalFiles        = totalFiles,
            TotalSizeBytes    = totalSizeBytes,
            FilesUploaded     = 0,
            FilesRemaining    = totalFiles,
            ProgressPercent   = 0,
            Status            = BackupStatus.Running
        };

        Upsert(state);
    }

    /// <summary>
    /// Called after each file is successfully copied.
    /// Increments FilesUploaded, decrements FilesRemaining, recalculates ProgressPercent.
    /// </summary>
    public void IncrementProgress(string name)
    {
        UpdateEntry(name, state =>
        {
            state.FilesUploaded++;
            state.FilesRemaining = state.TotalFiles - state.FilesUploaded;

            state.ProgressPercent = state.TotalFiles > 0
                ? (int)Math.Round((double)state.FilesUploaded / state.TotalFiles * 100)
                : 100;
        });
    }

    /// <summary>
    /// Marks the backup as successfully completed (progress = 100).
    /// </summary>
    public void MarkCompleted(string name)
    {
        UpdateEntry(name, state =>
        {
            state.FilesUploaded   = state.TotalFiles;
            state.FilesRemaining  = 0;
            state.ProgressPercent = 100;
            state.Status          = BackupStatus.Completed;
        });
    }

    /// <summary>
    /// Marks the backup as cancelled (e.g. business software detected).
    /// </summary>
    public void MarkCancelled(string name)
    {
        UpdateEntry(name, state =>
        {
            state.Status = BackupStatus.Cancelled;
        });
    }

    /// <summary>
    /// Marks the backup as errored.
    /// </summary>
    public void MarkError(string name)
    {
        UpdateEntry(name, state =>
        {
            state.Status = BackupStatus.Error;
        });
    }

    /// <summary>
    /// Marks the backup as paused.
    /// </summary>
    public void MarkPaused(string name)
    {
        UpdateEntry(name, state =>
        {
            state.Status = BackupStatus.Paused;
        });
    }

    /// <summary>
    /// Marks the backup as running again after a pause.
    /// </summary>
    public void MarkRunning(string name)
    {
        UpdateEntry(name, state =>
        {
            state.Status = BackupStatus.Running;
        });
    }

    /// <summary>
    /// Returns a snapshot of the current state for a given backup name.
    /// Returns null if not found.
    /// </summary>
    public BackupState? GetState(string name)
    {
        lock (_lock)
        {
            List<BackupState> states = ReadAll();
            return states.FirstOrDefault(s => s.Name == name);
        }
    }

    /// <summary>
    /// Returns all backup states currently in state.json.
    /// </summary>
    public List<BackupState> GetAllStates()
    {
        lock (_lock)
        {
            return ReadAll();
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies a mutation to an existing entry (matched by Name) and persists the result.
    /// If the entry doesn't exist yet, the mutation is silently skipped.
    /// </summary>
    private void UpdateEntry(string name, Action<BackupState> mutate)
    {
        lock (_lock)
        {
            List<BackupState> states = ReadAll();

            BackupState? target = states.FirstOrDefault(s => s.Name == name);
            if (target == null) return;

            mutate(target);
            WriteAll(states);
        }
    }

    /// <summary>
    /// Inserts or replaces a BackupState entry (matched by Name).
    /// </summary>
    private void Upsert(BackupState state)
    {
        lock (_lock)
        {
            List<BackupState> states = ReadAll();

            int index = states.FindIndex(s => s.Name == state.Name);
            if (index >= 0)
                states[index] = state;
            else
                states.Add(state);

            WriteAll(states);
        }
    }

    private List<BackupState> ReadAll()
    {
        if (!File.Exists(_stateFilePath))
            return new List<BackupState>();

        try
        {
            string json = File.ReadAllText(_stateFilePath);
            return JsonSerializer.Deserialize<List<BackupState>>(json, _jsonOptions)
                   ?? new List<BackupState>();
        }
        catch
        {
            // Corrupted file — start fresh rather than crashing
            return new List<BackupState>();
        }
    }

    private void WriteAll(List<BackupState> states)
    {
        string json = JsonSerializer.Serialize(states, _jsonOptions);
        File.WriteAllText(_stateFilePath, json);
    }

    private void EnsureDirectoryExists()
    {
        string? dir = Path.GetDirectoryName(_stateFilePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
    }
}
