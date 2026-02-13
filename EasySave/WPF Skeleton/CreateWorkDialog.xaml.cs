using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace WpfSkeleton
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
            
            // Désactiver le bouton au départ
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
            BrowseSourceButton.Content = LanguageManager.Get("CreateWork.Browse");
            BrowseDestinationButton.Content = LanguageManager.Get("CreateWork.Browse");
            CreateButton.Content = LanguageManager.Get("CreateWork.Create");
            CancelButton.Content = LanguageManager.Get("CreateWork.Cancel");
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = LanguageManager.Get("CreateWork.SourcePath"),
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                SourcePathTextBox.Text = System.IO.Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                ValidateForm();
            }
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = LanguageManager.Get("CreateWork.DestinationPath"),
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                DestinationPathTextBox.Text = System.IO.Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
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

            // Validate Work Name
            if (string.IsNullOrWhiteSpace(WorkNameTextBox.Text))
            {
                WorkNameError.Text = $"? {LanguageManager.Get("CreateWork.ErrorWorkName")} {LanguageManager.Get("CreateWork.ErrorRequired")}";
                WorkNameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                WorkNameError.Visibility = Visibility.Collapsed;
            }

            // Validate Source Path
            if (string.IsNullOrWhiteSpace(SourcePathTextBox.Text))
            {
                SourcePathError.Text = $"? {LanguageManager.Get("CreateWork.ErrorSourcePath")} {LanguageManager.Get("CreateWork.ErrorRequired")}";
                SourcePathError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Directory.Exists(SourcePathTextBox.Text))
            {
                SourcePathError.Text = $"? {LanguageManager.Get("CreateWork.ErrorSourceNotExists")} {LanguageManager.Get("CreateWork.ErrorNotExists")}";
                SourcePathError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                SourcePathError.Visibility = Visibility.Collapsed;
            }

            // Validate Destination Path
            if (string.IsNullOrWhiteSpace(DestinationPathTextBox.Text))
            {
                DestinationPathError.Text = $"? {LanguageManager.Get("CreateWork.ErrorDestPath")} {LanguageManager.Get("CreateWork.ErrorRequired")}";
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

            // Enable or disable the Create button
            CreateButton.IsEnabled = isValid;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // La validation est déjà faite, on peut créer directement
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
