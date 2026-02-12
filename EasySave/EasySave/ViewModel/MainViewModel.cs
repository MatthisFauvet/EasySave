using System.ComponentModel;
using System.Runtime.CompilerServices;

using EasySave.Model;
using EasySave.Service;

namespace EasySave.ViewModel;

public class MainViewModel : INotifyPropertyChanged
{
    //Just to clarify something, all these comments have been written by hand. I mean, I'm writing them with a
    //computer so it's not really handwritten, but it's not generate by IA.
    
    //First we define all private variable 
    private readonly IBackupService _backupService;

    private int _pageIndex; 
    
    private int _pageSize;
    
    private BackupCreateRequest _newBackupCreateRequest;

    public BackupCreateRequest BackupCreateRequest
    {
        get => _newBackupCreateRequest;
        set
        {
            _newBackupCreateRequest = value;
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

    public MainViewModel()
    {
        //Here is a template of how to use RelayCommand
        //LoadUserCommand = new RelayCommand(ReloadUser);
        //  Binding Name        Object     Method to execute 
        
        //TODO : Here, create an instance of BackupService.
        //Backups = LoadBackups();
        
        _backupService = new BackupService();
        CreateBackup();
    }

    private List<Backup> LoadBackups()
    {
        return _backupService.GetBackups(_pageIndex, _pageSize);
    }

    private void CreateBackup()
    {
        //BackupCreateRequest createBackup = new BackupCreateRequest();
        BackupCreateRequest = new BackupCreateRequest("Caca", "zizi", "iei", BackupType.Full);
    }
    
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

}