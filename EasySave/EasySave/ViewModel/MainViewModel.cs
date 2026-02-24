using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using EasySave.Model;
using EasySave.Service;
using EasySave.ViewModel.Command;

namespace EasySave.ViewModel;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IBackupService _backupService;
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

    private int _pageIndex = 0;
    private int _pageSize = 10;

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

    private bool _isExecuting;
    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            _isExecuting = value;
            OnPropertyChanged();
            ExecuteBackupsCommand.RaiseCanExecuteChanged();
        }
    }

    private string _executionStatus = "";
    public string ExecutionStatus
    {
        get => _executionStatus;
        private set
        {
            _executionStatus = value;
            OnPropertyChanged();
        }
    }

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

        ExecuteBackupsCommand = new RelayCommand(
            execute: ExecuteBackup,
            canExecute: () => !IsExecuting
        );

        CreateBackupCommand = new RelayCommand(
            execute: CreateBackup,
            canExecute: () => !IsExecuting
        );

        OpenCreateBackupDialogCommand = new RelayCommand(
            execute: () => OpenCreateBackupDialogRequested?.Invoke()
        );
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

    public void RemoveBackup(Backup backup)
    {
        if (backup == null)
            return;

        _backupService.RemoveBackup(backup);

        if (Backups.Contains(backup))
            Backups.Remove(backup);
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
        Task.Run(() =>
        {
            RunOnUiThread(() =>
            {
                IsExecuting = true;
                ExecutionStatus = "Backups running...";
            });

            try
            {
                bool success = _backupService.ExecuteBackup(Backups.ToList());

                RunOnUiThread(() =>
                {
                    ExecutionStatus = success
                        ? "All backups completed successfully."
                        : "Some backups failed. Check logs for details.";
                });
            }
            catch (Exception ex)
            {
                RunOnUiThread(() =>
                {
                    ExecutionStatus = $"Execution failed: {ex.Message}";
                });
            }
            finally
            {
                RunOnUiThread(() => IsExecuting = false);
            }
        });
    }

    private void RunOnUiThread(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            _dispatcher.Invoke(action);
        }
    }

    // ==========================
    // INotifyPropertyChanged
    // ==========================

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}