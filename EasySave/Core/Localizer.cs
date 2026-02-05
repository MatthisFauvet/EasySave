using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasySave.view;

namespace EasySave.Core
{
    public sealed class Localizer
    {
        private readonly Dictionary<string, string> _texts;

        public Localizer(UiLanguage lang)
        {
            string fileName = lang == UiLanguage.FR ? "texts.fr.json" : "texts.en.json";
            string path = Path.Combine(AppContext.BaseDirectory, "Resources", fileName);

            if (!File.Exists(path))
                throw new FileNotFoundException($"Localization file not found: {path}");

            string json = File.ReadAllText(path);

            _texts = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                     ?? new Dictionary<string, string>();
        }

        public string T(string key)
            => _texts.TryGetValue(key, out var value) ? value : key;
    }
}
