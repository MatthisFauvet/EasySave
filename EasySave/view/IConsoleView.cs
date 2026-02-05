namespace EasySave.view
{
    public enum UiLanguage
    {
        FR,
        EN
    }

    public enum BackupType
    {
        Full,
        Differential
    }

    public readonly record struct CreateBackupInput(
        string Name,
        string SourcePath,
        string DestinationPath,
        BackupType Type
    );

    public interface IConsoleView
    {
        UiLanguage AskLanguage();            // called once at app start
        int ShowMenu();                      // returns 0..4
        int AskBackupId(string purposeKey);  // "run" | "delete" | "rename"
        CreateBackupInput AskCreateBackupInfo();
        string AskNewBackupName();           // 🔹 NEW for rename

        void ShowMessage(string messageKey);
        void ShowError(string messageKey);
        void ShowBackupList(IEnumerable<(int Id, string Name, BackupType Type)> backups);

        void Pause();
    }
}
