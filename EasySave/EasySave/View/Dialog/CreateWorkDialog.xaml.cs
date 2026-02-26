using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace EasySave.View.Dialog
{
    public partial class CreateWorkDialog : Window
    {
        public string WorkName { get; private set; } = string.Empty;
        public string SourcePath { get; private set; } = string.Empty;
        public string DestinationPath { get; private set; } = string.Empty;
        public string SaveType { get; private set; } = "Complete";

        public CreateWorkDialog()
        {
            InitializeComponent();
            LoadTranslations();
            
            // D�sactiver le bouton au d�part
            CreateButton.IsEnabled = false;
            ValidateForm();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void LoadTranslations()
        {
            Title = LanguageManager.Get("CreateWork.Title");
            TitleText.Text = LanguageManager.Get("CreateWork.Title");
            WorkNameLabel.Text = LanguageManager.Get("CreateWork.WorkName");
            SourcePathLabel.Text = LanguageManager.Get("CreateWork.SourcePath");
            DestinationPathLabel.Text = LanguageManager.Get("CreateWork.DestinationPath");
            SaveTypeLabel.Text = LanguageManager.Get("CreateWork.SaveType");
            CompleteItem.Content = LanguageManager.Get("CreateWork.Complete");
            IncrementalItem.Content = LanguageManager.Get("CreateWork.Incremental");
            BrowseSourceFileButton.Content = LanguageManager.Get("CreateWork.BrowseFile");
            BrowseSourceFolderButton.Content = LanguageManager.Get("CreateWork.BrowseFolder");
            BrowseDestinationButton.Content = LanguageManager.Get("CreateWork.Browse");
            CreateButton.Content = LanguageManager.Get("CreateWork.Create");
            CancelButton.Content = LanguageManager.Get("CreateWork.Cancel");
        }

        private void BrowseSourceFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = LanguageManager.Get("CreateWork.SourcePath"),
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                SourcePathTextBox.Text = dialog.FileName;
                ValidateForm();
            }
        }

        private void BrowseSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = LanguageManager.Get("CreateWork.SourcePath")
            };

            if (dialog.ShowDialog() == true)
            {
                SourcePathTextBox.Text = dialog.FolderName;
                ValidateForm();
            }
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = LanguageManager.Get("CreateWork.DestinationPath")
            };

            if (dialog.ShowDialog() == true)
            {
                DestinationPathTextBox.Text = dialog.FolderName;
                ValidateForm();
            }
        }

        private void OnFieldChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(WorkNameTextBox.Text))
            {
                WorkNameError.Text = $"⚠ {LanguageManager.Get("CreateWork.ErrorWorkName")} {LanguageManager.Get("CreateWork.ErrorRequired")}";
                WorkNameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                WorkNameError.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrWhiteSpace(SourcePathTextBox.Text))
            {
                SourcePathError.Text = $"⚠ {LanguageManager.Get("CreateWork.ErrorSourcePath")} {LanguageManager.Get("CreateWork.ErrorRequired")}";
                SourcePathError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Directory.Exists(SourcePathTextBox.Text) && !File.Exists(SourcePathTextBox.Text))
            {
                SourcePathError.Text = $"? {LanguageManager.Get("CreateWork.ErrorSourceNotExists")} {LanguageManager.Get("CreateWork.ErrorNotExists")}";
                SourcePathError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                SourcePathError.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrWhiteSpace(DestinationPathTextBox.Text))
            {
                DestinationPathError.Text = $"⚠ {LanguageManager.Get("CreateWork.ErrorDestPath")} {LanguageManager.Get("CreateWork.ErrorRequired")}";
                DestinationPathError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Directory.Exists(DestinationPathTextBox.Text))
            {
                DestinationPathError.Text = $"? {LanguageManager.Get("CreateWork.ErrorDestNotExists")} {LanguageManager.Get("CreateWork.ErrorNotExists")}";
                DestinationPathError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (SourcePathTextBox.Text == DestinationPathTextBox.Text)
            {
                DestinationPathError.Text = $"? {LanguageManager.Get("CreateWork.ErrorSameFolder")}";
                DestinationPathError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                DestinationPathError.Visibility = Visibility.Collapsed;
            }

            CreateButton.IsEnabled = isValid;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // La validation est d�j� faite, on peut cr�er directement
            WorkName = WorkNameTextBox.Text;
            SourcePath = SourcePathTextBox.Text;
            DestinationPath = DestinationPathTextBox.Text;
            SaveType = SaveTypeComboBox.SelectedIndex == 0 ? "Complete" : "Incremental";

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
