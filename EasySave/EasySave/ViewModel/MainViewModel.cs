using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySave.Model;
using EasySave.Service;
using EasySave.ViewModel.Command;

namespace EasySave.ViewModel;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IBackupService _backupService;

    private int _pageIndex = 0;
    private int _pageSize = 50;

    // ==========================
    // Commands
    // ==========================

    public RelayCommand ExecuteBackupsCommand { get; }
    public RelayCommand CreateBackupCommand { get; }
    public RelayCommand OpenCreateBackupDialogCommand { get; }

    public event Action? OpenCreateBackupDialogRequested;

    // ==========================
    // Bindings
    // ==========================

    private BackupCreateRequest _backupCreateRequest;

    public BackupCreateRequest BackupCreateRequest
    {
        get => _backupCreateRequest;
        set
        {
            _backupCreateRequest = value;
            OnPropertyChanged();
        }
    }

    // 🔥 ObservableCollection pour le dynamisme
    public ObservableCollection<Backup> Backups { get; }

    public List<BackupType> BackupTypes { get; }

    // ==========================
    // Constructor
    // ==========================

    public MainViewModel()
    {
        _backupService = new BackupService();

        Backups = new ObservableCollection<Backup>();

        BackupCreateRequest = new BackupCreateRequest("", "", "", BackupType.Full);

        BackupTypes = Enum.GetValues(typeof(BackupType)).Cast<BackupType>().ToList();

        LoadBackups();

        OpenCreateBackupDialogCommand = new RelayCommand(() =>
            OpenCreateBackupDialogRequested?.Invoke());

        ExecuteBackupsCommand = new RelayCommand(ExecuteBackup);

        CreateBackupCommand = new RelayCommand(CreateBackup);
    }

    // ==========================
    // Methods
    // ==========================

    private void LoadBackups()
    {
        Backups.Clear();

        var backupsFromService = _backupService.GetBackups(_pageIndex, _pageSize);

        foreach (var backup in backupsFromService)
        {
            Backups.Add(backup);
        }
    }

    private void CreateBackup()
    {
        _backupService.CreateBackup(BackupCreateRequest);

        foreach (var backup in _backupService.GetBackups(_pageIndex, _pageSize))
        {
            if (!Backups.Contains(backup))
            {
                Backups.Add(backup);
            }
        }

        BackupCreateRequest = new BackupCreateRequest("", "", "", BackupType.Full);
    }


    private void ExecuteBackup()
    {
        _backupService.ExecuteBackup(Backups.ToList());
    }

    // ==========================
    // INotifyPropertyChanged
    // ==========================

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
