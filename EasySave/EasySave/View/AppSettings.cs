namespace EasySave.View
{
    public class AppSettings
    {
        // General
        public int AppTemplate { get; set; } = 1;
        public string AppTheme { get; set; } = "Light";
        public string Language { get; set; } = "Français";

        public bool AutoExecute { get; set; } = false; 

        public string LogFileType { get; set; } = "JSON";

        public List<string> PriorityExtensions { get; set; } = new();        
        public List<string> CustomExtensions { get; set; } = new();       
        public string DailyLogPath { get; set; } = "";       
    }
}
