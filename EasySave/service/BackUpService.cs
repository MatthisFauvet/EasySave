using EasyLog;
using EasyLog.entity;
using EasySave.Entity;
using EasySave.service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class BackupService : IBackupService
{
    private readonly Logger _logger;

    public BackupService(Logger logger)
    {
        _logger = logger;
    }

    //Prendre en parametre une liste de backup et executer la méthode de sauvegarde pour chacun d'entre eux et un string entre 0 et 2
    public bool ExecuteBackup(List<Backup> backups)
    {   
        bool isSuccessful = true;
        List<int> unvalidBackUps = new List<int>();

        Logger _logger = new Logger();
        _logger.InitWriters("logs", "Execution of backups");
        _logger.Log($"Starting execution of backups.", LogType.Info);

        foreach (Backup backup in backups)
        {

            try
            {
                ExecuteSingleBackup(backup);
                _logger.Log($"Backup (ID : {backup}) completed successfully.", LogType.Info);
            }
            catch (Exception ex)
            {
                _logger.Log($"Backup (ID : {backup}) failed : {ex.Message}", LogType.Error);
                unvalidBackUps.Add(backup.Id);
                isSuccessful = false;
            }
        }
        if ((!isSuccessful))
        {
            _logger.Log($"The following backup(s) failed to execute: {string.Join(", ", unvalidBackUps)}", LogType.Error);
        }
        _logger.Log($"Finished execution of backups.", LogType.Info);
        return isSuccessful;
    }

    private void ExecuteSingleBackup(Backup backup)
    {
        Logger _logger = new Logger();
        _logger.InitWriters(backup.DestinationFilePath, $"Execution du backup {backup.Id}");


        if (!Directory.Exists(backup.SourceFilePath)) {
            _logger.Log($"Source directory not found : {backup.SourceFilePath}", LogType.Error);
            throw new DirectoryNotFoundException($"Source not found : {backup.SourceFilePath}");
        }

        Directory.CreateDirectory(backup.DestinationFilePath);

        var sourceDirectory = new DirectoryInfo(backup.SourceFilePath);
        var files = sourceDirectory.GetFiles("*", SearchOption.AllDirectories);

        foreach (FileInfo sourceFile in files)
        {
            string relativePath = Path.GetRelativePath(
                backup.SourceFilePath,
                sourceFile.FullName
            );

            string destinationFilePath = Path.Combine(
                backup.DestinationFilePath,
                relativePath
            );

            Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath)!);

            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                sourceFile.CopyTo(destinationFilePath, true);
                stopwatch.Stop();

                _logger.Log(
                    $"sourcePath: {sourceFile.FullName}, " +
                    $"targetPath: {destinationFilePath}, " +
                    $"fileSize: {sourceFile.Length}, " +
                    $"transferTimeMs: {stopwatch.ElapsedMilliseconds}, " +
                    $"backupName: {backup.Name}",
                    LogType.Info
                    );

            }
            catch
            {
                stopwatch.Stop();

                _logger.Log(
                    $"sourcePath: {sourceFile.FullName}, " +
                    $"fileSize: {sourceFile.Length}, " +
                    $"transferTimeMs: {-1}, " +
                    $"backupName: {backup.Name}, ",
                    LogType.Error
                );
            }
        }
    }

}
