using System.Runtime.CompilerServices;

namespace EasySave.Entity;

public class Backup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SourceFilePath { get; set; }
    public string DestinationFilePath { get; set; }
    public DateTime LastBackupDate { get; set; }
}