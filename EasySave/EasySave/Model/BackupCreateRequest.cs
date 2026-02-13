namespace EasySave.Model;

public class BackupCreateRequest
{
    public BackupCreateRequest(string name, string sourceFilePath, string destinationFilePath, BackupType type)
    {
        Name = name;
        SourceFilePath = sourceFilePath;
        DestinationFilePath = destinationFilePath;
        Type = type;
    }

    public BackupCreateRequest()
    {
    }

    public string Name { get; set; }
    public string SourceFilePath { get; set; }
    public string DestinationFilePath { get; set; }
    public BackupType Type { get; set; }
};