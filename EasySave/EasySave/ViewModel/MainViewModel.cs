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

    public RelayCommand ExecuteBackupsCommand { get; }
    public RelayCommand CreateBackupCommand { get; }
    public RelayCommand OpenCreateBackupDialogCommand { get; }

    public event Action? OpenCreateBackupDialogRequested;
    public event Action<Backup>? BackupUpdated;

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

    public ObservableCollection<Backup> Backups { get; }

    public List<BackupType> BackupTypes { get; }

    public int MaxBandwidthKbps { get; set; } = 0;
    public string LogsDirectory { get; set; } = "logs";
    public string LogFileType { get; set; } = "JSON";

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


    private async void ExecuteBackup()
    {
        try
        {
            var selected = Backups
                .Where(b => b.IsSelected)
                .OrderByDescending(b => b.IsPriority)
                .ToList();

            if (!selected.Any()) return;

            _backupService.MaxBandwidthKbps = MaxBandwidthKbps;
            _backupService.LogsDirectory    = LogsDirectory;
            _backupService.LogFileType      = LogFileType;
            await _backupService.ExecuteBackupAsync(selected, b => BackupUpdated?.Invoke(b));
        }
        catch (Exception)
        {
            // Filet de sécurité : évite le crash d'un async void en cas d'exception inattendue
        }
    }

    public async Task ExecuteSingleJobAsync(Backup backup, Action<Backup>? onUpdate = null)
    {
        _backupService.MaxBandwidthKbps = MaxBandwidthKbps;
        _backupService.LogsDirectory    = LogsDirectory;
        _backupService.LogFileType      = LogFileType;
        await _backupService.ExecuteBackupAsync([backup], onUpdate);
    }

    public void ToggleBackup(Backup backup)
    {
        backup.Status = backup.Status == BackupStatus.InProgress
            ? BackupStatus.Paused
            : BackupStatus.InProgress;
        _backupService.UpdateBackup(backup);
    }

    public void StartBackup(Backup backup)
    {
        backup.Status = BackupStatus.InProgress;
        _backupService.UpdateBackup(backup);
    }

    public void DeleteBackup(Backup backup)
    {
        _backupService.RemoveBackup(backup);
        Backups.Remove(backup);
    }

    public void UpdateBackup(Backup backup)
    {
        _backupService.UpdateBackup(backup);
    }

    public List<HistoryEntry> GetHistory()
    {
        return _backupService.GetHistory().OrderByDescending(e => e.StartTime).ToList();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
