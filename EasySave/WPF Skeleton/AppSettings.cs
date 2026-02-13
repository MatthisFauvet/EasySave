namespace WpfSkeleton
{
    public class AppSettings
    {
        // General
        public int AppTemplate { get; set; } = 1; // 1 = Navbar, 2 = Tabs
        public string AppTheme { get; set; } = "Light";
        public string Language { get; set; } = "Français";

        // Saves
        public bool AutoExecute { get; set; } = false; // false = manual, true = auto

        // Logs
        public string LogFileType { get; set; } = "JSON"; // JSON ou XML
    }
}
