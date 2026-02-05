using System.Runtime.CompilerServices;

namespace EasySave.Entity;

public class Backup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SourceFilePath { get; set; }
    public string DestinationFilePath { get; set; }
    public string TimeStamp { get; set; }
    public Backup(int id, string name, string sourceFilePath, string destinationFilePath) { 
        Id = id;
        Name = name;
        SourceFilePath = sourceFilePath;
        DestinationFilePath = destinationFilePath;
    }
}