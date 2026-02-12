using System.Windows;
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
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WorkNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(SourcePathTextBox.Text) ||
                string.IsNullOrWhiteSpace(DestinationPathTextBox.Text))
            {
                MessageBox.Show(
                    LanguageManager.Get("CreateWork.ErrorEmptyFields"),
                    LanguageManager.Get("CreateWork.ErrorTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

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
