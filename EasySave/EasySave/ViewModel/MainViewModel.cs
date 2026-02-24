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

    // Capture the UI thread dispatcher at construction time
    // The ViewModel is always created on the UI thread, so this is safe
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

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
    // Tracks whether backups are currently running
    // Used to disable the Execute button while running
    private bool _isExecuting;
    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            _isExecuting = value;
            OnPropertyChanged();
            // Tell the command to re-evaluate CanExecute
            // so the button enables/disables automatically
            ExecuteBackupsCommand.RaiseCanExecuteChanged();
        }
    }
    // Feedback message shown in the UI during/after execution
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


    //  ObservableCollection pour le dynamisme
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

        // Pass canExecute so the button disables while backups are running
        ExecuteBackupsCommand = new RelayCommand(
            execute: ExecuteBackup,
            canExecute: () => !IsExecuting
        );

        CreateBackupCommand = new RelayCommand(
            execute: CreateBackup,
            canExecute: () => !IsExecuting
        );

        // ✅ AJOUTER : Commande pour ouvrir le dialogue de création
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
        // Fire and forget on a background thread
        // 'async void' is acceptable here because this is a UI event handler
        // We don't want to block the UI thread while backups run
        Task.Run(async () =>
        {
            // Switch IsExecuting to true on the UI thread
            // This disables the button immediately
            RunOnUiThread(() =>
            {
                IsExecuting = true;
                ExecutionStatus = "Backups running...";
            });

            try
            {
                // This now runs ALL backups in parallel on background threads
                // The UI remains fully responsive during this call
                bool success = _backupService.ExecuteBackup(Backups.ToList());

                // All backups done — update UI from UI thread
                RunOnUiThread(() =>
                {
                    ExecutionStatus = success
                        ? "All backups completed successfully."
                        : "Some backups failed. Check logs for details.";

                    // ❌ SUPPRIMER CETTE LIGNE - Elle cause les doublons!
                    // LoadBackups();
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
                // Always re-enable the button, even if something threw
                RunOnUiThread(() => IsExecuting = false);
            }
        });
    }
    /// <summary>
    /// Helper to safely dispatch any action back to the UI thread.
    /// Checks first if we're already on the UI thread to avoid
    /// unnecessary dispatching overhead.
    /// </summary>
    private void RunOnUiThread(Action action)
    {
        if (_dispatcher.CheckAccess())
        {
            // Already on UI thread — run directly
            action();
        }
        else
        {
            // On background thread — marshal to UI thread
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
