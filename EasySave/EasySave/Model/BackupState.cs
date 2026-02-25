using System.Text.Json.Serialization;

namespace EasySave.Model;

/// <summary>
/// Represents the live state of one backup job, persisted in state.json.
/// </summary>
public class BackupState
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("totalFiles")]
    public int TotalFiles { get; set; }

    [JsonPropertyName("totalSizeBytes")]
    public long TotalSizeBytes { get; set; }

    [JsonPropertyName("filesUploaded")]
    public int FilesUploaded { get; set; }

    [JsonPropertyName("filesRemaining")]
    public int FilesRemaining { get; set; }

    [JsonPropertyName("progressPercent")]
    public int ProgressPercent { get; set; }

    [JsonPropertyName("status")]
    public BackupStatus Status { get; set; } = BackupStatus.Idle;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BackupStatus
{
    Idle,
    Running,
    Paused,
    Cancelled,
    Error,
    Completed
}