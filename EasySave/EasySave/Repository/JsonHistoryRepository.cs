using System.IO;
using System.Text.Json;
using EasySave.Model;

namespace EasySave.Repository;

public class JsonHistoryRepository
{
    private static readonly string FilePath =
        Path.Combine(AppContext.BaseDirectory, "history", "history.json");

    private readonly object _sync = new();

    public void AddEntry(HistoryEntry entry)
    {
        lock (_sync)
        {
            var entries = LoadFromFile();
            entries.Add(entry);
            SaveToFile(entries);
        }
    }

    public List<HistoryEntry> GetAll()
    {
        return LoadFromFile();
    }

    private List<HistoryEntry> LoadFromFile()
    {
        if (!File.Exists(FilePath))
            return [];
        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<HistoryEntry>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private void SaveToFile(List<HistoryEntry> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}
