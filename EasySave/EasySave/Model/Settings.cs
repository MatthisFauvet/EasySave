namespace EasySave.Model;

public class Settings
{
    public int AppTemplate { get; set; }
    public string AppTheme { get; set; }
    public string Language { get; set; }
    public bool AutoExecute { get; set; }
    public string LogFileType { get; set; }

    public Settings()
    {
    }
}