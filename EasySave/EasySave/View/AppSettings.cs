namespace EasySave.View
{
    public class AppSettings
    {
        // General
        public int AppTemplate { get; set; } = 1;
        public string AppTheme { get; set; } = "Light";
        public string Language { get; set; } = "French";

        // Saves
        public int MaxBandwidthKbps { get; set; } = 0;

        // Logs
        public string LogFileType { get; set; } = "JSON";
        // Relatif à l'AppBase ou absolu (ex: /app/logs pour Docker)
        public string LogsDirectory { get; set; } = "logs";
    }
}
