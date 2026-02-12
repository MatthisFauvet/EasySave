using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EasySave.Model;
using EasySave.Service;
using EasySave.ViewModel.Command;

namespace EasySave.ViewModel;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IBackupService _backupService;

    private int _pageIndex;
    private int _pageSize;

    public RelayCommand ExecuteBackupsCommand { get; set; }
    public RelayCommand CreateBackupCommand { get; set; }

    private BackupCreateRequest _newBackupCreateRequest;

    public BackupCreateRequest BackupCreateRequest
    {
        get => _newBackupCreateRequest;
        set
        {
            _newBackupCreateRequest = value;
            OnPropertyChanged();
        }
    }

    private List<Backup> _backups;

    public List<Backup> Backups
    {
        get => _backups;
        set
        {
            _backups = value;
            OnPropertyChanged();
        }
    }

    // Liste des types pour le ComboBox
    public List<BackupType> BackupTypes { get; set; }

    public MainViewModel()
    {
        _backupService = new BackupService();
        Backups = LoadBackups();

        // Initialiser les types pour le ComboBox
        BackupTypes = Enum.GetValues(typeof(BackupType)).Cast<BackupType>().ToList();

        // Initialiser un objet vide pour le formulaire
        BackupCreateRequest = new BackupCreateRequest("", "", "", BackupType.Full);

        ExecuteBackupsCommand = new RelayCommand(ExecuteBackup);
        CreateBackupCommand = new RelayCommand(CreateBackup);
    }

    private void ExecuteBackup()
    {
        var selectedBackups = Backups.Where(b => b.IsSelected).ToList();
        _backupService.ExecuteBackup(selectedBackups);
    }

    private List<Backup> LoadBackups()
    {
        return _backupService.GetBackups(_pageIndex, _pageSize);
    }

    private void CreateBackup()
    {
        // Validation minimale
        if (string.IsNullOrWhiteSpace(BackupCreateRequest.Name) ||
            string.IsNullOrWhiteSpace(BackupCreateRequest.SourceFilePath) ||
            string.IsNullOrWhiteSpace(BackupCreateRequest.DestinationFilePath))
        {
            Console.WriteLine("Tous les champs doivent être remplis !");
            return;
        }
        
        _backupService.CreateBackup(BackupCreateRequest);

        LoadBackups();
        BackupCreateRequest = new BackupCreateRequest("", "", "", BackupType.Full);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
