using EasySave.Entity;

namespace EasySave.view;

public class CreateBackupRequest
{
    public CreateBackupRequest(string name, string sourceFilePath, string destinationFilePath, BackupType type)
    {
        Name = name;
        SourceFilePath = sourceFilePath;
        DestinationFilePath = destinationFilePath;
        Type = type;
    }
    public string Name { get; set; }
    public string SourceFilePath { get; set; }
    public string DestinationFilePath { get; set; }
    public BackupType Type { get; set; }
    
};