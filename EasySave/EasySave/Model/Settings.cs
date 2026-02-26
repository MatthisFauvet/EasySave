namespace EasySave.Model;

/// <summary>
/// Defines where daily log files are written.
/// Local  → only on the current machine (DailyLogPath)
/// Docker → only on the shared Docker volume (DockerLogPath)
/// Both   → on both simultaneously
/// </summary>
public enum LogStorageMode
{
    Local,
    Docker,
    Both
}

public class Settings
{
    // ── Apparence & langue ───────────────────────────────────────────────────
    public int    AppTemplate { get; set; } = 1;        // 1 = Navbar, 2 = Tabs
    public string AppTheme    { get; set; } = "Light";
    public string Language    { get; set; } = "Français";

    // ── Exécution ────────────────────────────────────────────────────────────
    public bool AutoExecute { get; set; } = false;

    // ── Logs ─────────────────────────────────────────────────────────────────
    public string LogFileType { get; set; } = "JSON";   // "JSON" ou "XML"

    /// <summary>Where to write the daily log file.</summary>
    public LogStorageMode LogStorageMode { get; set; } = LogStorageMode.Local;

    /// <summary>Local folder for the daily log (used when mode is Local or Both).</summary>
    public string DailyLogPath { get; set; } = string.Empty;

    /// <summary>
    /// UNC path to the Docker shared folder (used when mode is Docker or Both).
    /// Example: \\192.168.1.10\easysave-logs
    /// The JsonFileWriter writes directly here — no API involved.
    /// </summary>
    public string DockerLogPath { get; set; } = string.Empty;

    // ── Fichiers prioritaires ────────────────────────────────────────────────
    public List<string> PriorityExtensions { get; set; } = new();
    public List<string> CustomExtensions   { get; set; } = new();

    public Settings() { }
}