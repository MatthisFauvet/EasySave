using System.Runtime.CompilerServices;

namespace EasySave.Entity;

public class Backup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SourceFilePath { get; set; }
    public string DestinationFilePath { get; set; }
    public string TimeStamp { get; set; }
    public BackupType Type { get; set; }
    public Backup(int id, string name, string sourceFilePath, string destinationFilePath, string type) { 
        Id                  = id;
        Name                = name;
        SourceFilePath      = sourceFilePath;
        DestinationFilePath = destinationFilePath;
        Type                = BackupTypeHelper.FromString(type);
    }
}
