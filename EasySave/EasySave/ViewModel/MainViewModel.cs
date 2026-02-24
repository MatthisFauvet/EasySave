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

    // 1-based paging (IMPORTANT)
    private int _pageIndex = 1;
    private int _pageSize = 5;

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

    // Backups = page courante (on garde ton nom pour éviter de toucher la view)
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

        OpenCreateBackupDialogCommand = new RelayCommand(() =>
            OpenCreateBackupDialogRequested?.Invoke());

        ExecuteBackupsCommand = new RelayCommand(ExecuteBackup);
        CreateBackupCommand = new RelayCommand(CreateBackup);

        // NEW: pagination commands
        PreviousPageCommand = new RelayCommand(
            execute: () => LoadPage(PageIndex - 1),
            canExecute: () => CanGoPrevious);

        NextPageCommand = new RelayCommand(
            execute: () => LoadPage(PageIndex + 1),
            canExecute: () => CanGoNext);

        // First load
        LoadPage(1);
    }

    // ==========================
    // Methods
    // ==========================
    private void LoadPage(int pageIndex)
    {
        if (pageIndex < 1) pageIndex = 1;

        // IMPORTANT: needs a paged result (Items + TotalCount)
        var page = _backupService.GetBackupsPage(pageIndex, PageSize);

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
        // Here, you execute the current page only.
        // If you want "execute all backups", you'd need a separate service call to fetch all.
        _backupService.ExecuteBackup(Backups.ToList());
    }

    // ==========================
    // INotifyPropertyChanged
    // ==========================
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}