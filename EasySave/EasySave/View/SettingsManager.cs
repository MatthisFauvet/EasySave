using System;
using System.IO;
using System.Text.Json;
using EasySave.Model;

namespace EasySave.View
{
    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EasySave",
            "settings.json"
        );

        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<Settings>(json)
                                   ?? new Settings();

                    
                    settings.PriorityExtensions ??= new List<string>();
                    settings.CustomExtensions ??= new List<string>();

                    return settings;
                }
            }
            catch (Exception)
            {
                // En cas d'erreur, retourner les paramètres par défaut
            }
            return new Settings();
        }

        public static void Save(Settings settings)
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsPath)!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception)
            {
                // Gestion silencieuse des erreurs de sauvegarde
            }
        }
    }
}
