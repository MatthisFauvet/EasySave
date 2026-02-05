using EasySave.Core;
using EasySave.Entity;
using EasySave.view;
using System;

namespace EasySave.Views
{
    public sealed class ConsoleView : IConsoleView
    {
        private UiLanguage _lang;
        private Localizer? _loc;
        private bool _initialized;

        public UiLanguage AskLanguage()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("==================================");
                Console.WriteLine("             EASY SAVE");
                Console.WriteLine("==================================");
                Console.WriteLine();
                Console.WriteLine("Choose your language / Choisissez votre langue :");
                Console.WriteLine("1 - Français");
                Console.WriteLine("2 - English");
                Console.WriteLine();
                Console.Write("Choice / Choix (1-2): ");

                var input = Console.ReadLine()?.Trim();

                if (input == "1") return UiLanguage.FR;
                if (input == "2") return UiLanguage.EN;

                Console.WriteLine();
                Console.WriteLine("Invalid choice / Choix invalide.");
                Console.WriteLine("Press Enter...");
                Console.ReadLine();
            }
        }

        public int ShowMenu()
        {
            EnsureInitialized();

            while (true)
            {
                Console.Clear();
                PrintHeader();

                Console.WriteLine(_loc!.T("menu.0"));
                Console.WriteLine(_loc!.T("menu.1"));
                Console.WriteLine(_loc!.T("menu.2"));
                Console.WriteLine(_loc!.T("menu.3"));
                Console.WriteLine(_loc!.T("menu.4"));
                Console.WriteLine();
                Console.Write(_loc!.T("prompt.choice"));

                var input = Console.ReadLine()?.Trim();
                if (int.TryParse(input, out int choice) && choice is >= 0 and <= 4)
                    return choice;

                ShowError("error.invalidChoice");
                Pause();
            }
        }

        public int AskBackupId(string purposeKey)
        {
            EnsureInitialized();

            string key = purposeKey switch
            {
                "run" => "prompt.backupId.run",
                "delete" => "prompt.backupId.delete",
                "rename" => "prompt.backupId.rename",
                _ => "prompt.backupId.run"
            };


            while (true)
            {
                Console.Write(_loc!.T(key));
                var input = Console.ReadLine()?.Trim();

                if (int.TryParse(input, out int id) && id >= 1)
                    return id;

                ShowError("error.invalidChoice");
            }
        }

        public CreateBackupInput AskCreateBackupInfo()
        {
            EnsureInitialized();

            string name = AskRequired("prompt.name");
            string source = AskRequired("prompt.source");
            string dest = AskRequired("prompt.dest");
            BackupType type = AskBackupType();

            return new CreateBackupInput(name, source, dest, type);
        }
        public string AskNewBackupName()
        {
            EnsureInitialized();
            return AskRequired("prompt.newName");
        }

        public void ShowMessage(string messageKey)
        {
            EnsureInitialized();
            Console.WriteLine(_loc!.T(messageKey));
        }

        public void ShowError(string messageKey)
        {
            EnsureInitialized();
            Console.WriteLine();
            Console.WriteLine(_loc!.T(messageKey));
            Console.WriteLine();
        }
        public void ShowBackupList(List<Backup> backups)
        {
            EnsureInitialized();
            Console.Clear();

            Console.WriteLine("==================================");
            Console.WriteLine($"             {_loc!.T("app.title")}");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine(_loc!.T("msg.listTitle"));
            Console.WriteLine();

            bool any = false;

            Console.WriteLine("ID | Name                 | Type");
            Console.WriteLine("----------------------------------------");

            foreach (Backup b in backups)
            {
                any = true;
                Console.WriteLine($"{b.Id,2} | {b.Name,-20} }");
            }

            if (!any)
                Console.WriteLine(_loc!.T("msg.noBackups"));

            Console.WriteLine();
        }

        public void Pause()
        {
            EnsureInitialized();
            Console.WriteLine(_loc!.T("pause"));
            Console.ReadLine();
        }

        // ---------------- Helpers ----------------

        private void EnsureInitialized()
        {
            if (_initialized) return;

            _lang = AskLanguage();
            _loc = new Localizer(_lang);
            _initialized = true;
        }

        private void PrintHeader()
        {
            Console.WriteLine("==================================");
            Console.WriteLine($"             {_loc!.T("app.title")}");
            Console.WriteLine($"            {_loc!.T("menu.title")}");
            Console.WriteLine("==================================");
            Console.WriteLine();
        }

        private string AskRequired(string promptKey)
        {
            while (true)
            {
                Console.Write(_loc!.T(promptKey));
                var input = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                    return input.Trim();

                ShowError("error.invalidChoice");
            }
        }

        private BackupType AskBackupType()
        {
            while (true)
            {
                Console.Write(_loc!.T("prompt.type"));
                var input = Console.ReadLine()?.Trim();

                if (input == "1") return BackupType.Full;
                if (input == "2") return BackupType.Differential;

                ShowError("error.invalidChoice");
            }
        }
    }
}
