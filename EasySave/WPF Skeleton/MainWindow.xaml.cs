using System.Windows;
using System.Windows.Controls;

namespace WpfSkeleton
{
    public partial class MainWindow : Window
    {
        private AppSettings _settings;
        private bool _isInitializing = true;

        public MainWindow()
        {
            InitializeComponent();

            _settings = SettingsManager.Load();
            
            LanguageManager.LoadLanguage(_settings.Language);
            ThemeManager.ApplyTheme(_settings.AppTheme);
            
            ApplyTemplate(_settings.AppTemplate);
            UpdateUILanguage();
            SectionHome();

            _isInitializing = false;
        }

        #region Template Management

        private void ApplyTemplate(int templateNumber)
        {
            if (templateNumber == 1)
            {
                Template1.Visibility = Visibility.Visible;
                Template2.Visibility = Visibility.Collapsed;
            }
            else
            {
                Template1.Visibility = Visibility.Collapsed;
                Template2.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region UI Language Update

        private void UpdateUILanguage()
        {
            Title = LanguageManager.Get("App.Title");
            
            NavTitle1.Text = LanguageManager.Get("Navigation.Title");
            BtnHome1.Content = LanguageManager.Get("Navigation.Home");
            BtnSaves1.Content = LanguageManager.Get("Navigation.Saves");
            BtnHistory1.Content = LanguageManager.Get("Navigation.History");
            BtnSettings1.Content = LanguageManager.Get("Navigation.Settings");
            
            BtnHome2.Content = LanguageManager.Get("Navigation.Home");
            BtnSaves2.Content = LanguageManager.Get("Navigation.Saves");
            BtnHistory2.Content = LanguageManager.Get("Navigation.History");
            BtnSettings2.Content = LanguageManager.Get("Navigation.Settings");
        }

        #endregion

        #region Navigation

        private void NavHome_Click(object sender, RoutedEventArgs e)
        {
            SectionHome();
        }

        private void NavSaves_Click(object sender, RoutedEventArgs e)
        {
            SectionSaves();
        }

        private void NavHistory_Click(object sender, RoutedEventArgs e)
        {
            SectionHistory();
        }

        private void NavSettings_Click(object sender, RoutedEventArgs e)
        {
            SectionSettings();
        }

        #endregion

        #region Sections

        private void SectionHome()
        {
            var content = new StackPanel();
            
            var title = new TextBlock
            {
                Text = LanguageManager.Get("Home.Title"),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var description = new TextBlock
            {
                Text = LanguageManager.Get("Home.Welcome"),
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 0)
            };
            description.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(description);

            SetContent(content);
        }

        private void SectionSaves()
        {
            var content = new StackPanel();

            var title = new TextBlock
            {
                Text = LanguageManager.Get("Saves.Title"),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var btnCreateWork = new Button
            {
                Content = LanguageManager.Get("Saves.CreateWork"),
                Width = 200,
                Height = 40,
                Margin = new Thickness(0, 10, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            btnCreateWork.Click += BtnCreateWork_Click;
            content.Children.Add(btnCreateWork);

            var btnExecuteWorks = new Button
            {
                Content = LanguageManager.Get("Saves.ExecuteWorks"),
                Width = 200,
                Height = 40,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            btnExecuteWorks.Click += BtnExecuteWorks_Click;
            content.Children.Add(btnExecuteWorks);

            var listTitle = new TextBlock
            {
                Text = LanguageManager.Get("Saves.WorksList"),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 10)
            };
            listTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(listTitle);

            var worksList = new ListBox
            {
                Height = 250,
                Margin = new Thickness(0, 0, 0, 10)
            };
            worksList.Items.Add($"{LanguageManager.Get("Saves.Work")} 1 - {LanguageManager.Get("Saves.Pending")}");
            worksList.Items.Add($"{LanguageManager.Get("Saves.Work")} 2 - {LanguageManager.Get("Saves.InProgress")}");
            worksList.Items.Add($"{LanguageManager.Get("Saves.Work")} 3 - {LanguageManager.Get("Saves.Completed")}");
            content.Children.Add(worksList);

            SetContent(content);
        }

        private void SectionHistory()
        {
            var content = new StackPanel();

            var title = new TextBlock
            {
                Text = LanguageManager.Get("History.Title"),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var description = new TextBlock
            {
                Text = LanguageManager.Get("History.Description"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 10)
            };
            description.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(description);

            var historyList = new ListBox
            {
                Height = 400,
                Margin = new Thickness(0, 10, 0, 0)
            };
            historyList.Items.Add($"2024-01-15 10:30 - {LanguageManager.Get("Saves.Work")} 1 - {LanguageManager.Get("History.Success")}");
            historyList.Items.Add($"2024-01-15 09:15 - {LanguageManager.Get("Saves.Work")} 2 - {LanguageManager.Get("History.Success")}");
            historyList.Items.Add($"2024-01-14 16:45 - {LanguageManager.Get("Saves.Work")} 3 - {LanguageManager.Get("History.Fail")}");
            historyList.Items.Add($"2024-01-14 14:20 - {LanguageManager.Get("Saves.Work")} 1 - {LanguageManager.Get("History.Abort")}");
            historyList.Items.Add($"2024-01-13 11:30 - {LanguageManager.Get("Saves.Work")} 2 - {LanguageManager.Get("History.Success")}");
            content.Children.Add(historyList);

            SetContent(content);
        }

        private void SectionSettings()
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var content = new StackPanel();

            var title = new TextBlock
            {
                Text = LanguageManager.Get("Settings.Title"),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var generalTitle = new TextBlock
            {
                Text = LanguageManager.Get("Settings.General"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 10)
            };
            generalTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(generalTitle);

            var templatePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            var templateLabel = new TextBlock { Text = LanguageManager.Get("Settings.TemplateApp"), Width = 150, VerticalAlignment = VerticalAlignment.Center };
            templateLabel.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            templatePanel.Children.Add(templateLabel);
            var comboTemplate = new ComboBox { Width = 200 };
            comboTemplate.Items.Add(LanguageManager.Get("Settings.Template1"));
            comboTemplate.Items.Add(LanguageManager.Get("Settings.Template2"));
            comboTemplate.SelectedIndex = _settings.AppTemplate - 1;
            comboTemplate.SelectionChanged += ComboTemplate_Changed;
            templatePanel.Children.Add(comboTemplate);
            content.Children.Add(templatePanel);

            var themePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            var themeLabel = new TextBlock { Text = LanguageManager.Get("Settings.ThemeApp"), Width = 150, VerticalAlignment = VerticalAlignment.Center };
            themeLabel.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            themePanel.Children.Add(themeLabel);
            var comboTheme = new ComboBox { Width = 200 };
            comboTheme.Items.Add(LanguageManager.Get("Settings.Light"));
            comboTheme.Items.Add(LanguageManager.Get("Settings.Dark"));
            comboTheme.SelectedIndex = _settings.AppTheme == "Light" ? 0 : 1;
            comboTheme.SelectionChanged += ComboTheme_Changed;
            themePanel.Children.Add(comboTheme);
            content.Children.Add(themePanel);

            var languagePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            var languageLabel = new TextBlock { Text = LanguageManager.Get("Settings.Language"), Width = 150, VerticalAlignment = VerticalAlignment.Center };
            languageLabel.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            languagePanel.Children.Add(languageLabel);
            var comboLanguage = new ComboBox { Width = 200 };
            comboLanguage.Items.Add(LanguageManager.Get("Settings.French"));
            comboLanguage.Items.Add(LanguageManager.Get("Settings.English"));
            comboLanguage.SelectedIndex = _settings.Language == "Français" ? 0 : 1;
            comboLanguage.SelectionChanged += ComboLanguage_Changed;
            languagePanel.Children.Add(comboLanguage);
            content.Children.Add(languagePanel);

            var savesTitle = new TextBlock
            {
                Text = LanguageManager.Get("Settings.Saves"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10)
            };
            savesTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(savesTitle);

            var executionPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            var executionLabel = new TextBlock { Text = LanguageManager.Get("Settings.ExecutionMode"), Width = 150, VerticalAlignment = VerticalAlignment.Center };
            executionLabel.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            executionPanel.Children.Add(executionLabel);
            var comboExecution = new ComboBox { Width = 200 };
            comboExecution.Items.Add(LanguageManager.Get("Settings.Manual"));
            comboExecution.Items.Add(LanguageManager.Get("Settings.Auto"));
            comboExecution.SelectedIndex = _settings.AutoExecute ? 1 : 0;
            comboExecution.SelectionChanged += ComboExecution_Changed;
            executionPanel.Children.Add(comboExecution);
            content.Children.Add(executionPanel);

            var logsTitle = new TextBlock
            {
                Text = LanguageManager.Get("Settings.Logs"),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10)
            };
            logsTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(logsTitle);

            var logTypePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            var logTypeLabel = new TextBlock { Text = LanguageManager.Get("Settings.FileType"), Width = 150, VerticalAlignment = VerticalAlignment.Center };
            logTypeLabel.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            logTypePanel.Children.Add(logTypeLabel);
            var comboLogType = new ComboBox { Width = 200 };
            comboLogType.Items.Add(LanguageManager.Get("Settings.JSON"));
            comboLogType.Items.Add(LanguageManager.Get("Settings.XML"));
            comboLogType.SelectedIndex = _settings.LogFileType == "JSON" ? 0 : 1;
            comboLogType.SelectionChanged += ComboLogType_Changed;
            logTypePanel.Children.Add(comboLogType);
            content.Children.Add(logTypePanel);

            scrollViewer.Content = content;
            SetContent(scrollViewer);
        }

        private void SetContent(UIElement content)
        {
            if (Template1.Visibility == Visibility.Visible)
            {
                ContentTemplate1.Content = content;
            }
            else
            {
                ContentTemplate2.Content = content;
            }
        }

        #endregion

        #region Event Handlers - Saves Section

        private void BtnCreateWork_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateWorkDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                MessageBox.Show(
                    LanguageManager.Get("CreateWork.SuccessMessage") + 
                    $"\n\n{LanguageManager.Get("CreateWork.WorkName")} {dialog.WorkName}" +
                    $"\n{LanguageManager.Get("CreateWork.SourcePath")} {dialog.SourcePath}" +
                    $"\n{LanguageManager.Get("CreateWork.DestinationPath")} {dialog.DestinationPath}" +
                    $"\n{LanguageManager.Get("CreateWork.SaveType")} {dialog.SaveType}",
                    LanguageManager.Get("CreateWork.SuccessTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void BtnExecuteWorks_Click(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        #region Event Handlers - Settings Section

        private void ComboTemplate_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            var combo = (ComboBox)sender;
            _settings.AppTemplate = combo.SelectedIndex + 1;
            SettingsManager.Save(_settings);

            var currentSection = ContentTemplate1.Content ?? ContentTemplate2.Content;
            ApplyTemplate(_settings.AppTemplate);
            
            if (currentSection != null)
            {
                if (Template1.Visibility == Visibility.Visible)
                {
                    ContentTemplate1.Content = currentSection;
                }
                else
                {
                    ContentTemplate2.Content = currentSection;
                }
            }
        }

        private void ComboTheme_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            var combo = (ComboBox)sender;
            _settings.AppTheme = combo.SelectedIndex == 0 ? "Light" : "Dark";
            SettingsManager.Save(_settings);
            ThemeManager.ApplyTheme(_settings.AppTheme);
        }

        private void ComboLanguage_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            var combo = (ComboBox)sender;
            _settings.Language = combo.SelectedIndex == 0 ? "Français" : "English";
            SettingsManager.Save(_settings);
            LanguageManager.LoadLanguage(_settings.Language);
            
            UpdateUILanguage();
            
            if (ContentTemplate1.Content != null || ContentTemplate2.Content != null)
            {
                SectionSettings();
            }
        }

        private void ComboExecution_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            var combo = (ComboBox)sender;
            _settings.AutoExecute = combo.SelectedIndex == 1;
            SettingsManager.Save(_settings);
        }

        private void ComboLogType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            var combo = (ComboBox)sender;
            _settings.LogFileType = combo.SelectedIndex == 0 ? "JSON" : "XML";
            SettingsManager.Save(_settings);
        }

        #endregion
    }
}
