using EasyLog;
using EasySave.Entity;
using EasySave.service;
using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static void Main()
    {
        // Crée les dossiers temporaires pour le test
        string tempSource = Path.Combine(Path.GetTempPath(), "BackupSource");
        string tempDest = Path.Combine(Path.GetTempPath(), "BackupDest");
        Directory.CreateDirectory(tempSource);
        Directory.CreateDirectory(tempDest);

        // Crée un fichier test
        File.WriteAllText(Path.Combine(tempSource, "test.txt"), "Ceci est un fichier de test");

        // Crée le dossier de logs
        string logFolder = Path.Combine(Path.GetTempPath(), "logs");
        Directory.CreateDirectory(logFolder);

        // Initialise le logger
        Logger logger = new Logger();
        logger.InitWriters(logFolder, "TestBackup");

        // Initialise le service
        BackupService backupService = new BackupService(logger);

        // Prépare le backup
        Backup backup = new Backup(1, "TestBackup", tempSource, tempDest);

        // Exécute le backup
        bool result = backupService.ExecuteBackup(new List<Backup> { backup });

        Console.WriteLine($"Backup terminé avec succès ? {result}");

        // Vérifie que le fichier a bien été copié
        string copiedFile = Path.Combine(tempDest, "test.txt");
        if (File.Exists(copiedFile))
        {
            Console.WriteLine("Le fichier a été copié avec succès !");
        }
        else
        {
            Console.WriteLine("Erreur : le fichier n'a pas été copié.");
        }
    }
}
