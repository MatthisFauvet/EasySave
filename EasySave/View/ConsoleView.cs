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

        /// <summary>
        ///     With this method we can change software language Fr or En
        /// </summary>
        /// <returns>Return enum object type</returns>
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

        /// <summary>
        /// This method display a list of actions you can perform in the sofwatre
        /// </summary>
        /// <returns>
        ///     0.
        ///     1.
        ///     2.
        ///     3.
        ///     4.
        /// </returns>
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

        public CreateBackupRequest AskCreateBackupInfo()
        {
            EnsureInitialized();

            string name = AskRequired("prompt.name");
            string source = AskRequired("prompt.source");
            string dest = AskRequired("prompt.dest");
            BackupType type = AskBackupType();

            return new CreateBackupRequest(name, source, dest, type);
        }

        public List<int> SelectBackupToExecute()
        {
            while (true)
            {
                Console.WriteLine(_loc!.T("prompt.ask.backup.id.exec"));
                Console.WriteLine(_loc!.T("prompt.ask.backup.id.exec.choice1"));
                Console.WriteLine(_loc!.T("prompt.ask.backup.id.exec.choice2"));

                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    ShowError("error.invalidChoice");
                    continue;
                }
                
                if (input.Contains('-'))
                {
                    var parts = input.Split('-', StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int start) &&
                        int.TryParse(parts[1], out int end) &&
                        start >= 1 &&
                        end >= start)
                    {
                        return Enumerable.Range(start, end - start + 1).ToList();
                    }
                }
                
                else if (input.Contains(';'))
                {
                    var parts = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    var result = new List<int>();
                    bool valid = true;

                    foreach (var part in parts)
                    {
                        if (int.TryParse(part, out int value) && value >= 1)
                            result.Add(value);
                        else
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid && result.Count > 0)
                        return result;
                }
                
                else if (int.TryParse(input, out int single) && single >= 1)
                {
                    return new List<int> { single };
                }

                ShowError("error.invalidChoice");
            }
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
                Console.WriteLine($"{b.Id,2} | {b.Name,-20} | {b.Type, -10}");
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
                if (input == "2") return BackupType.Sequential;

                ShowError("error.invalidChoice");
            }
        }
    }
}
