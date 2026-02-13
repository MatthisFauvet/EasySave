using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace EasySave.View
{
    public static class LanguageManager
    {
        private static Dictionary<string, JsonElement> _translations = new();
        private static string _currentLanguage = "Français";

        public static void LoadLanguage(string language)
        {
            _currentLanguage = language;
            string fileName = language == "Français" ? "fr.json" : "en.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);

            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var doc = JsonDocument.Parse(json);
                    _translations.Clear();

                    foreach (var property in doc.RootElement.EnumerateObject())
                    {
                        _translations[property.Name] = property.Value.Clone();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading language file: {ex.Message}");
            }
        }

        public static string Get(string key)
        {
            try
            {
                var keys = key.Split('.');
                if (keys.Length == 0) return key;

                if (!_translations.ContainsKey(keys[0]))
                    return key;

                JsonElement current = _translations[keys[0]];

                for (int i = 1; i < keys.Length; i++)
                {
                    if (current.TryGetProperty(keys[i], out JsonElement next))
                    {
                        current = next;
                    }
                    else
                    {
                        return key;
                    }
                }

                return current.GetString() ?? key;
            }
            catch
            {
                return key;
            }
        }

        public static string CurrentLanguage => _currentLanguage;
    }
}
