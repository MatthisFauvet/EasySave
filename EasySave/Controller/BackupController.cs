using System.Runtime.InteropServices;
using EasyLog;
using EasySave.Entity;
using EasySave.Repository;
using EasySave.service;
using EasySave.view;
using EasySave.Views;

namespace EasySave.Controller;

public class BackupController
{

    private IConsoleView _consoleView;
    
    private IBackupRepository  _backupRepository;
    
    private IBackupRepository  _backupJsonRepository;
    
    private IBackupService _backupService;

    private List<Backup> _backups;
    
    public BackupController()
    {
        _consoleView = new ConsoleView();
        _backupRepository = new BackupRepository();
        _backupJsonRepository = new JsonBackupRepository();
        _backupService = new BackupService();
        _backups = new List<Backup>();
        Program();
    }

    private void Program()
    {
        
        _backups = _backupJsonRepository.GetAll();
        
        while (true)
        {
            
            int actionToPerform = _consoleView.ShowMenu();

            switch (actionToPerform)
            {
                //Create backup
                case 0:
                    CreateBackupRequest createBackupRequest = _consoleView.AskCreateBackupInfo();
                    _backupRepository.Add(createBackupRequest);
                    _backupJsonRepository.Add(createBackupRequest);
                    _backups = _backupRepository.GetAll();
                    break;

                //List backup
                case 1:
                    _backups = _backupJsonRepository.GetAll();
                    _consoleView.ShowBackupList(_backups);
                    _consoleView.Pause();
                    break;

                //Delete
                case 2:
                    _consoleView.ShowBackupList(_backups);
                    _backupJsonRepository.Remove(_consoleView.AskBackupId("delete"));
                    _consoleView.Pause();
                    break;

                //Run backup
                case 3:
                    _consoleView.ShowBackupList(_backups);
                    List<int> ids = _consoleView.SelectBackupToExecute();
                    List<Backup> backupsToRun = [];
                    if (_backups.Count > 0)
                    {
                        backupsToRun.AddRange(ids.Select(id => _backupJsonRepository.GetById(id)));
                    }
                    _backupService.ExecuteBackup(backupsToRun);
                    _consoleView.Pause();
                    break;
                
                //Rename
                case 4:
                    Console.WriteLine("This feature is not available yet, following the MOSCOW logic it's not important");
                    break;
            }
        }
    }
}