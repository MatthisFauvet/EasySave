namespace EasySave.Model;

public class HistoryEntry
{
    public string BackupName { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string DestinationPath { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public long DurationMs { get; set; }
    public long TotalSizeBytes { get; set; }
    public int FileCount { get; set; }
    public BackupStatus Status { get; set; }
    public bool HasWarnings { get; set; }
    public string? ErrorMessage { get; set; }
}
