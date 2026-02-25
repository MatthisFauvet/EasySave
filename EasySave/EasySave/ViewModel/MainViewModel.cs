using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using EasySave.Model;
using EasySave.Service;
using EasySave.ViewModel.Command;

namespace EasySave.ViewModel;

/// <summary>
/// Wraps a Backup with live progress data polled from StateService.
/// Implements INotifyPropertyChanged so the UI updates automatically.
/// </summary>
public class BackupProgressItem : INotifyPropertyChanged
{
    private readonly StateService _stateService;

    public Backup Backup { get; }

    private int _progress;
    public int Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); }
    }

    private int _filesUploaded;
    public int FilesUploaded
    {
        get => _filesUploaded;
        set { _filesUploaded = value; OnPropertyChanged(); }
    }

    private int _totalFiles;
    public int TotalFiles
    {
        get => _totalFiles;
        set { _totalFiles = value; OnPropertyChanged(); }
    }

    private BackupStatus _status = BackupStatus.Idle;
    public BackupStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(IsRunning));
        }
    }

    public bool IsPaused  => _status == BackupStatus.Paused;
    public bool IsRunning => _status == BackupStatus.Running;

    public BackupProgressItem(Backup backup, StateService stateService)
    {
        Backup = backup;
        _stateService = stateService;
    }

    /// <summary>
    /// Polls state.json for this backup and refreshes all bound properties.
    /// Called by the DispatcherTimer on the UI thread — no marshalling needed.
    /// </summary>
    public void Refresh()
    {
        var state = _stateService.GetState(Backup.Name);
        if (state == null) return;

        Progress      = state.ProgressPercent;
        FilesUploaded = state.FilesUploaded;
        TotalFiles    = state.TotalFiles;
        Status        = state.Status;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IBackupService _backupService;
    private readonly StateService   _stateService;

    // Timer that polls state.json every 500ms and refreshes all BackupProgressItems
    private readonly DispatcherTimer _progressTimer;

    // Capture the UI thread dispatcher at construction time
    private readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

    private int _pageIndex = 1;
    private int _pageSize  = 5;
    private int _totalCount;

    // ==========================
    // Commands
    // ==========================

    public RelayCommand ExecuteBackupsCommand          { get; }
    public RelayCommand CreateBackupCommand            { get; }
    public RelayCommand OpenCreateBackupDialogCommand  { get; }
    public RelayCommand NextPageCommand                { get; }
    public RelayCommand PreviousPageCommand            { get; }

    public event Action? OpenCreateBackupDialogRequested;

    // ==========================
    // Bindings
    // ==========================

    private BackupCreateRequest _backupCreateRequest;
    public BackupCreateRequest BackupCreateRequest
    {
        get => _backupCreateRequest;
        set { _backupCreateRequest = value; OnPropertyChanged(); }
    }

    private string _searchQuery = "";
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (_searchQuery == value) return;
            _searchQuery = value;
            OnPropertyChanged();
            LoadPage(1);
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

            // Start / stop polling with execution state
            if (_isExecuting)
                _progressTimer.Start();
            else
                _progressTimer.Stop();
        }
    }

    private string _executionStatus = "";
    public string ExecutionStatus
    {
        get => _executionStatus;
        private set { _executionStatus = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Observable list of progress wrappers, one per backup on the current page.
    /// The view binds directly to these items.
    /// </summary>
    public ObservableCollection<BackupProgressItem> BackupItems { get; } = new();

    // Keep a plain Backup list for service calls (ExecuteBackup, etc.)
    public ObservableCollection<Backup> Backups { get; } = new();

    public List<BackupType> BackupTypes { get; }

    public int PageIndex
    {
        get => _pageIndex;
        private set
        {
            if (_pageIndex == value) return;
            _pageIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize == value) return;
            if (value < 1) value = 1;
            _pageSize = value;
            OnPropertyChanged();
            LoadPage(1);
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (_totalCount == value) return;
            _totalCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalPages));
            OnPropertyChanged(nameof(CanGoNext));
        }
    }

    public int TotalPages    => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool CanGoPrevious => PageIndex > 1;
    public bool CanGoNext     => PageIndex < TotalPages;

    // ==========================
    // Constructor
    // ==========================

    public MainViewModel()
    {
        _backupService = new BackupService();
        _stateService  = new StateService();

        BackupCreateRequest = new BackupCreateRequest("", "", "", BackupType.Full);
        BackupTypes = Enum.GetValues(typeof(BackupType)).Cast<BackupType>().ToList();

        // Poll every 500ms — fast enough to feel live, cheap enough not to stutter
        _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _progressTimer.Tick += (_, _) =>
        {
            foreach (var item in BackupItems)
                item.Refresh();
        };

        PreviousPageCommand = new RelayCommand(
            execute:    () => LoadPage(PageIndex - 1),
            canExecute: () => CanGoPrevious);

        NextPageCommand = new RelayCommand(
            execute:    () => LoadPage(PageIndex + 1),
            canExecute: () => CanGoNext);

        ExecuteBackupsCommand = new RelayCommand(
            execute:    ExecuteBackup,
            canExecute: () => !IsExecuting);

        CreateBackupCommand = new RelayCommand(
            execute:    CreateBackup,
            canExecute: () => !IsExecuting);

        OpenCreateBackupDialogCommand = new RelayCommand(
            execute: () => OpenCreateBackupDialogRequested?.Invoke());

        LoadPage(1);
    }

    // ==========================
    // Pause / Resume
    // ==========================

    public void PauseBackup(Backup backup)
    {
        _backupService.PauseBackup(backup);
    }

    public void ResumeBackup(Backup backup)
    {
        _backupService.ResumeBackup(backup);
    }

    // ==========================
    // Methods
    // ==========================

    private void LoadPage(int pageIndex)
    {
        if (pageIndex < 1) pageIndex = 1;

        var page = _backupService.SearchBackupsPage(SearchQuery, pageIndex, PageSize);

        Backups.Clear();
        BackupItems.Clear();

        foreach (var b in page.Items)
        {
            Backups.Add(b);
            BackupItems.Add(new BackupProgressItem(b, _stateService));
        }

        TotalCount = page.TotalCount;
        PageIndex  = page.PageIndex;

        PreviousPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
    }

    public void RemoveBackup(Backup backup)
    {
        if (backup == null) return;

        _backupService.RemoveBackup(backup);

        var item = BackupItems.FirstOrDefault(i => i.Backup == backup);
        if (item != null) BackupItems.Remove(item);

        if (Backups.Contains(backup)) Backups.Remove(backup);
    }

    private void CreateBackup()
    {
        _backupService.CreateBackup(BackupCreateRequest);
        LoadPage(PageIndex);
        BackupCreateRequest = new BackupCreateRequest("", "", "", BackupType.Full);
    }

    private void ExecuteBackup()
    {
        Task.Run(() =>
        {
            RunOnUiThread(() =>
            {
                IsExecuting     = true;
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
                RunOnUiThread(() => ExecutionStatus = $"Execution failed: {ex.Message}");
            }
            finally
            {
                RunOnUiThread(() => IsExecuting = false);
            }
        });
    }

    private void RunOnUiThread(Action action)
    {
        if (_dispatcher.CheckAccess()) action();
        else _dispatcher.Invoke(action);
    }

    // ==========================
    // INotifyPropertyChanged
    // ==========================

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
