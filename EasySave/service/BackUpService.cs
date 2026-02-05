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

    public bool ExecuteBackup(List<int> backupsIds)
    {   
        bool isSuccessful = true;
        List<int> unvalidBackUps = new List<int>();
        foreach (int id in backupsIds)
        {
            _logger.InitWriters();

            try
            {
                ExecuteSingleBackup(id);
                _logger.Log($"Backup (ID : {id}) completed successfully.", LogType.Info);
            }
            catch (Exception ex)
            {
                _logger.Log($"Backup (ID : {id}) failed : {ex.Message}", LogType.Error);
                isSuccessful = false;
            }
        }
        return isSuccessful;
    }

    private void ExecuteSingleBackup(int backupId)
    {
        _logger.InitWriters();
        Backup backup = BackupRepository.GetBackupById(backupId);

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
                    $"transferTimeMs: -1, " +
                    $"backupName: {backup.Name}, ",
                    LogType.Error
                );
            }
        }
    }

}
