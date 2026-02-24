using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Mime;
using System.Runtime.CompilerServices;
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
    private readonly Dispatcher _dispatcher = MediaTypeNames.Application.Current.Dispatcher;

    private int _pageIndex = 0;
    private int _pageSize = 10;
    private int _totalCount;


    // ==========================
    // Commands
    // ==========================
    
    public RelayCommand ExecuteBackupsCommand { get; }
    public RelayCommand CreateBackupCommand { get; }
    public RelayCommand OpenCreateBackupDialogCommand { get; }

    // NEW: Pagination commands
    public RelayCommand NextPageCommand { get; }
    public RelayCommand PreviousPageCommand { get; }

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

            // Recherche live : revenir à la page 1 quand la saisie change
            LoadPage(1);
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

    // NEW: Paging state exposed for the View bindings (PageIndex / TotalPages)
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

            // Reset to first page when page size changes
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

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool CanGoPrevious => PageIndex > 1;
    public bool CanGoNext => PageIndex < TotalPages;

    // ==========================
    // Constructor
    // ==========================

    public MainViewModel()
    {
        _backupService = new BackupService();

        Backups = new ObservableCollection<Backup>();
        BackupCreateRequest = new BackupCreateRequest("", "", "", BackupType.Full);
        BackupTypes = Enum.GetValues(typeof(BackupType)).Cast<BackupType>().ToList();
        
        PreviousPageCommand = new RelayCommand(
            execute: () => LoadPage(PageIndex - 1),
            canExecute: () => CanGoPrevious);

        NextPageCommand = new RelayCommand(
            execute: () => LoadPage(PageIndex + 1),
            canExecute: () => CanGoNext);
        
        LoadPage(1);
        
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
    private void LoadPage(int pageIndex)
    {
        if (pageIndex < 1) pageIndex = 1;

        // IMPORTANT: needs a paged result (Items + TotalCount)
        var page = _backupService.SearchBackupsPage(SearchQuery, pageIndex, PageSize);

        Backups.Clear();
        foreach (var b in page.Items)
            Backups.Add(b);

        TotalCount = page.TotalCount;
        PageIndex = page.PageIndex;

        // Refresh buttons enabled state
        PreviousPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
    }

    private void CreateBackup()
    {
        _backupService.CreateBackup(BackupCreateRequest);

        // After create, reload current page to stay consistent (ordering, counts, etc.)
        LoadPage(PageIndex);

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
