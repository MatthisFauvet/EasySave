namespace EasySave.Model;

public class Backup
{
    public Backup()
    {
    }

    public Backup(int id, string name, string sourceFilePath, string destinationFilePath, DateTime lastBackupDateTime, BackupType type)
    {
        Id = id;
        Name = name;
        SourceFilePath = sourceFilePath;
        DestinationFilePath = destinationFilePath;
        LastBackupDateTime = lastBackupDateTime;
        Type = type;
    }

    public Backup(int id, string name, string sourceFilePath, string destinationFilePath, string type)
    { 
        Id = id;
        Name = name;
        SourceFilePath = sourceFilePath;
        DestinationFilePath = destinationFilePath;
        Type = BackupTypeHelper.FromString(type);
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string SourceFilePath { get; set; }
    public string DestinationFilePath { get; set; }
    public DateTime LastBackupDateTime { get; set; }
    public BackupType Type { get; set; }
}