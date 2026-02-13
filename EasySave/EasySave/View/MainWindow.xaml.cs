using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using EasySave.Model;
using EasySave.View.Dialog;
using EasySave.ViewModel;

namespace EasySave.View
{
    public partial class MainWindow : Window
    {
        private AppSettings _settings;
        private bool _isInitializing = true;
        private string _currentSection = "home";
        private MainViewModel _vm;

        public MainWindow()
        {
            _vm = new MainViewModel();
            DataContext = _vm;

            _vm.OpenCreateBackupDialogRequested += () =>
            {
                BtnCreateWork_Click();
            };
            
            InitializeComponent();

            _settings = SettingsManager.Load();

            LanguageManager.LoadLanguage(_settings.Language);
            ThemeManager.ApplyTheme(_settings.AppTheme);

            ApplyTemplate(_settings.AppTemplate);
            UpdateUILanguage();
            UpdateStorageInfo();
            SectionHome();
            
            
            _isInitializing = false;
        }

        #region Custom Title Bar

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                BtnMaximize_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

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

        #region Storage Info

        private void UpdateStorageInfo()
        {
            try
            {
                var systemDrive = DriveInfo.GetDrives()[0];
                
                if (systemDrive.IsReady)
                {
                    long totalBytes = systemDrive.TotalSize;
                    long freeBytes = systemDrive.AvailableFreeSpace;
                    long usedBytes = totalBytes - freeBytes;

                    double usedGB = usedBytes / (1024.0 * 1024.0 * 1024.0);
                    double totalGB = totalBytes / (1024.0 * 1024.0 * 1024.0);
                    double percentageUsed = (usedBytes / (double)totalBytes) * 100;

                    StorageProgressBar.Value = percentageUsed;
                    StorageTextBlock.Text = $"{usedGB:F1} Go / {totalGB:F1} Go";
                }
                else
                {
                    StorageTextBlock.Text = LanguageManager.Get("Storage.NotAvailable");
                }
            }
            catch
            {
                StorageTextBlock.Text = LanguageManager.Get("Storage.ReadError");
            }
        }

        #endregion

        #region UI Language Update

        private void UpdateUILanguage()
        {
            Title = LanguageManager.Get("App.Title");
            TitleVersion.Text = "  " + LanguageManager.Get("App.Version");

            NavTitle1.Text = LanguageManager.Get("Navigation.Title");
            BtnHome1.Content = LanguageManager.Get("Navigation.Home");
            BtnSaves1.Content = LanguageManager.Get("Navigation.Saves");
            BtnHistory1.Content = LanguageManager.Get("Navigation.History");
            BtnSettings1.Content = LanguageManager.Get("Navigation.Settings");

            BtnHome2.Content = LanguageManager.Get("Navigation.Home");
            BtnSaves2.Content = LanguageManager.Get("Navigation.Saves");
            BtnHistory2.Content = LanguageManager.Get("Navigation.History");
            BtnSettings2.Content = LanguageManager.Get("Navigation.Settings");

            StorageTitleBlock.Text = LanguageManager.Get("Storage.Title");
            
            // Refresh current section to update its content
            switch (_currentSection)
            {
                case "home": SectionHome(); break;
                case "saves": SectionSaves(); break;
                case "history": SectionHistory(); break;
                case "settings": SectionSettings(); break;
            }
        }

        #endregion

        #region Navigation

        private void UpdateNavStyles(string section)
        {
            _currentSection = section;

            // Template 1 buttons
            BtnHome1.Style = (Style)FindResource(section == "home" ? "NavButtonActive" : "NavButton");
            BtnSaves1.Style = (Style)FindResource(section == "saves" ? "NavButtonActive" : "NavButton");
            BtnHistory1.Style = (Style)FindResource(section == "history" ? "NavButtonActive" : "NavButton");
            BtnSettings1.Style = (Style)FindResource(section == "settings" ? "NavButtonActive" : "NavButton");

            // Template 2 buttons
            BtnHome2.Style = (Style)FindResource(section == "home" ? "NavButtonActive" : "NavButton");
            BtnSaves2.Style = (Style)FindResource(section == "saves" ? "NavButtonActive" : "NavButton");
            BtnHistory2.Style = (Style)FindResource(section == "history" ? "NavButtonActive" : "NavButton");
            BtnSettings2.Style = (Style)FindResource(section == "settings" ? "NavButtonActive" : "NavButton");
        }

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
            UpdateNavStyles("home");

            var content = new StackPanel();

            // Header row
            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 28) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var headerStack = new StackPanel();
            var title = new TextBlock
            {
                Text = LanguageManager.Get("Home.Title"),
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            headerStack.Children.Add(title);

            var subtitle = new TextBlock
            {
                Text = LanguageManager.Get("Home.Welcome"),
                FontSize = 13,
                Margin = new Thickness(0, 6, 0, 0),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            subtitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            headerStack.Children.Add(subtitle);
            headerGrid.Children.Add(headerStack);
            content.Children.Add(headerGrid);

            // Stats cards
            var statsGrid = new Grid { Margin = new Thickness(0, 0, 0, 24) };
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            AddStatCard(statsGrid, 0, _vm.Backups.Count().ToString(), LanguageManager.Get("Home.AmountOfBackup"), "#34D399");
            //AddStatCard(statsGrid, 2, "128", LanguageManager.Get("Home.Completed"), "#60A5FA");

            content.Children.Add(statsGrid);

            // Recent activity section
            var recentTitle = new TextBlock
            {
                Text = LanguageManager.Get("Home.RecentActivity"),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            recentTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(recentTitle);

            // Recent items in cards
            //AddRecentItem(content, "Sauvegarde Documents", "Termine - il y a 2h", "success");
            
            SetContent(content);
            // AddConsoleLog("Navigation vers Accueil.", "info");
        }

        private void AddStatCard(Grid parent, int column, string value, string label, string colorHex)
        {
            var card = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 14, 16, 14),
                BorderThickness = new Thickness(1)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var stack = new StackPanel();

            var valBlock = new TextBlock
            {
                Text = value,
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            stack.Children.Add(valBlock);

            var labelBlock = new TextBlock
            {
                Text = label,
                FontSize = 11,
                Margin = new Thickness(0, 4, 0, 0),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            labelBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            stack.Children.Add(labelBlock);

            card.Child = stack;
            Grid.SetColumn(card, column);
            parent.Children.Add(card);
        }

        private void AddRecentItem(StackPanel parent, string name, string status, string type)
        {
            var card = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 0, 0, 6),
                BorderThickness = new Thickness(1)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderSubtleBrush");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text = name,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            nameBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            Grid.SetColumn(nameBlock, 0);
            grid.Children.Add(nameBlock);

            Color statusColor;
            switch (type)
            {
                case "success": statusColor = (Color)ColorConverter.ConvertFromString("#34D399"); break;
                case "warning": statusColor = (Color)ColorConverter.ConvertFromString("#FBBF24"); break;
                case "error": statusColor = (Color)ColorConverter.ConvertFromString("#F87171"); break;
                default: statusColor = (Color)ColorConverter.ConvertFromString("#60A5FA"); break;
            }

            var statusBlock = new TextBlock
            {
                Text = status,
                FontSize = 11,
                Foreground = new SolidColorBrush(statusColor),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            Grid.SetColumn(statusBlock, 1);
            grid.Children.Add(statusBlock);

            card.Child = grid;
            parent.Children.Add(card);
        }

        private void SectionSaves()
        {
            UpdateNavStyles("saves");

            var content = new StackPanel();

            var title = new TextBlock
            {
                Text = LanguageManager.Get("Saves.Title"),
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 6),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var subtitle = new TextBlock
            {
                Text = LanguageManager.Get("Saves.Subtitle"),
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 24),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            subtitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            content.Children.Add(subtitle);

            // Action buttons
            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var btnCreateWork = new Button
            {
                Content = LanguageManager.Get("Saves.CreateWork"),
                Style = (Style)FindResource("PrimaryButton"),
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            btnCreateWork.SetBinding(Button.CommandProperty, 
                new Binding("OpenCreateBackupDialogCommand"));
            
            btnPanel.Children.Add(btnCreateWork);

            var btnExecuteWorks = new Button
            {
                Content = LanguageManager.Get("Saves.ExecuteWorks"),
                Margin = new Thickness(0, 0, 10, 0)
            };
            
            btnExecuteWorks.SetBinding(Button.CommandProperty, 
                new Binding("ExecuteBackupsCommand"));
            
            btnPanel.Children.Add(btnExecuteWorks);
            content.Children.Add(btnPanel);

            // Works list header
            var listTitle = new TextBlock
            {
                Text = LanguageManager.Get("Saves.WorksList"),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 10, 0, 10),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            listTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(listTitle);

            var tmpBackups = _vm.Backups;
            foreach (var tmpBackup in tmpBackups)
            {
                AddWorkItemFromBackup(content, tmpBackup);
            }

            _vm.Backups.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (Backup newBackup in e.NewItems)
                    {
                        AddWorkItemFromBackup(content, newBackup);
                    }
                }
            };
            
            SetContent(content);
            // AddConsoleLog("Navigation vers Sauvegardes.", "info");
        }
        
        private void AddWorkItemFromBackup(StackPanel content, Backup backup)
        {
            AddWorkItem(
                content,
                $"{backup.Name}",
                $"{backup.Type}",
                "warning",
                100
            );
        }

        private void AddWorkItem(StackPanel parent, string name, string status, string type, int progress)
        {
            var card = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(1)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var stack = new StackPanel();

            // Top row: name + status
            var topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text = name,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            nameBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            Grid.SetColumn(nameBlock, 0);
            topRow.Children.Add(nameBlock);

            Color statusColor;
            switch (type)
            {
                case "success": statusColor = (Color)ColorConverter.ConvertFromString("#34D399"); break;
                case "warning": statusColor = (Color)ColorConverter.ConvertFromString("#FBBF24"); break;
                default: statusColor = (Color)ColorConverter.ConvertFromString("#60A5FA"); break;
            }

            // Status badge
            var badge = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 3, 8, 3),
                Background = new SolidColorBrush(Color.FromArgb(30, statusColor.R, statusColor.G, statusColor.B))
            };
            var statusText = new TextBlock
            {
                Text = status,
                FontSize = 10,
                Foreground = new SolidColorBrush(statusColor),
                FontWeight = FontWeights.SemiBold,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            badge.Child = statusText;
            Grid.SetColumn(badge, 1);
            topRow.Children.Add(badge);
            stack.Children.Add(topRow);

            // Progress bar
            if (progress > 0 && progress < 100)
            {
                var progressBar = new ProgressBar
                {
                    Value = progress,
                    Maximum = 100,
                    Height = 3,
                    Margin = new Thickness(0, 8, 0, 0)
                };
                stack.Children.Add(progressBar);
            }

            card.Child = stack;
            parent.Children.Add(card);
        }

        private void SectionHistory()
        {
            UpdateNavStyles("history");

            var content = new StackPanel();

            var title = new TextBlock
            {
                Text = LanguageManager.Get("History.Title"),
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 6),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var description = new TextBlock
            {
                Text = LanguageManager.Get("History.Description"),
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 24),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            description.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            content.Children.Add(description);

            // History items as cards
            //AddHistoryItem(content, "2024-01-15 10:30", $"{LanguageManager.Get("Saves.Work")} 1", LanguageManager.Get("History.Success"), "success", "1.2 Go", "2m 34s");
           
            SetContent(content);
            // AddConsoleLog("Navigation vers Historique.", "info");
        }

        private void AddHistoryItem(StackPanel parent, string date, string work, string status, string type, string size, string duration)
        {
            var card = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 0, 0, 4),
                BorderThickness = new Thickness(1)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderSubtleBrush");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) }); // Date
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); // Size
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) }); // Duration
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Status

            var dateBlock = new TextBlock
            {
                Text = date,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("MonoFont")
            };
            dateBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            Grid.SetColumn(dateBlock, 0);
            grid.Children.Add(dateBlock);

            var workBlock = new TextBlock
            {
                Text = work,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            workBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            Grid.SetColumn(workBlock, 1);
            grid.Children.Add(workBlock);

            var sizeBlock = new TextBlock
            {
                Text = size,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("MonoFont")
            };
            sizeBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            Grid.SetColumn(sizeBlock, 2);
            grid.Children.Add(sizeBlock);

            var durationBlock = new TextBlock
            {
                Text = duration,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("MonoFont")
            };
            durationBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            Grid.SetColumn(durationBlock, 3);
            grid.Children.Add(durationBlock);

            Color statusColor;
            switch (type)
            {
                case "success": statusColor = (Color)ColorConverter.ConvertFromString("#34D399"); break;
                case "warning": statusColor = (Color)ColorConverter.ConvertFromString("#FBBF24"); break;
                case "error": statusColor = (Color)ColorConverter.ConvertFromString("#F87171"); break;
                default: statusColor = (Color)ColorConverter.ConvertFromString("#60A5FA"); break;
            }

            var statusBadge = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
                HorizontalAlignment = HorizontalAlignment.Right,
                Background = new SolidColorBrush(Color.FromArgb(25, statusColor.R, statusColor.G, statusColor.B))
            };
            var statusText = new TextBlock
            {
                Text = status,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(statusColor),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            statusBadge.Child = statusText;
            Grid.SetColumn(statusBadge, 4);
            grid.Children.Add(statusBadge);

            card.Child = grid;
            parent.Children.Add(card);
        }

        private void SectionSettings()
        {
            UpdateNavStyles("settings");

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var content = new StackPanel();

            var title = new TextBlock
            {
                Text = LanguageManager.Get("Settings.Title"),
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 6),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var subtitle = new TextBlock
            {
                Text = LanguageManager.Get("Settings.Subtitle"),
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 28),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            subtitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            content.Children.Add(subtitle);

            // -- General section --
            AddSectionHeader(content, LanguageManager.Get("Settings.General"));

            var generalCard = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 16, 20, 16),
                Margin = new Thickness(0, 0, 0, 20),
                BorderThickness = new Thickness(1)
            };
            generalCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            generalCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var generalStack = new StackPanel();

            // Template
            var comboTemplate = new ComboBox { Width = 220, MinHeight = 32 };
            comboTemplate.Items.Add(LanguageManager.Get("Settings.Template1"));
            comboTemplate.Items.Add(LanguageManager.Get("Settings.Template2"));
            comboTemplate.SelectedIndex = _settings.AppTemplate - 1;
            comboTemplate.SelectionChanged += ComboTemplate_Changed;
            AddSettingRow(generalStack, LanguageManager.Get("Settings.TemplateApp"), LanguageManager.Get("Settings.TemplateDescription"), comboTemplate);

            // Theme
            var comboTheme = new ComboBox { Width = 220, MinHeight = 32 };
            comboTheme.Items.Add(LanguageManager.Get("Settings.Light"));
            comboTheme.Items.Add(LanguageManager.Get("Settings.Dark"));
            comboTheme.SelectedIndex = _settings.AppTheme == "Light" ? 0 : 1;
            comboTheme.SelectionChanged += ComboTheme_Changed;
            AddSettingRow(generalStack, LanguageManager.Get("Settings.ThemeApp"), LanguageManager.Get("Settings.ThemeDescription"), comboTheme);

            // Language
            var comboLanguage = new ComboBox { Width = 220, MinHeight = 32 };
            comboLanguage.Items.Add(LanguageManager.Get("Settings.French"));
            comboLanguage.Items.Add(LanguageManager.Get("Settings.English"));
            comboLanguage.SelectedIndex = _settings.Language == "Fran\u00e7ais" ? 0 : 1;
            comboLanguage.SelectionChanged += ComboLanguage_Changed;
            AddSettingRow(generalStack, LanguageManager.Get("Settings.Language"), LanguageManager.Get("Settings.LanguageDescription"), comboLanguage, false);

            generalCard.Child = generalStack;
            content.Children.Add(generalCard);

            // -- Saves section --
            AddSectionHeader(content, LanguageManager.Get("Settings.Saves"));

            var savesCard = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 16, 20, 16),
                Margin = new Thickness(0, 0, 0, 20),
                BorderThickness = new Thickness(1)
            };
            savesCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            savesCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var savesStack = new StackPanel();

            var comboExecution = new ComboBox { Width = 220, MinHeight = 32 };
            comboExecution.Items.Add(LanguageManager.Get("Settings.Manual"));
            comboExecution.Items.Add(LanguageManager.Get("Settings.Auto"));
            comboExecution.SelectedIndex = _settings.AutoExecute ? 1 : 0;
            comboExecution.SelectionChanged += ComboExecution_Changed;
            AddSettingRow(savesStack, LanguageManager.Get("Settings.ExecutionMode"), LanguageManager.Get("Settings.ExecutionDescription"), comboExecution, false);

            savesCard.Child = savesStack;
            content.Children.Add(savesCard);

            // -- Logs section --
            AddSectionHeader(content, LanguageManager.Get("Settings.Logs"));

            var logsCard = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 16, 20, 16),
                Margin = new Thickness(0, 0, 0, 20),
                BorderThickness = new Thickness(1)
            };
            logsCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            logsCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var logsStack = new StackPanel();

            var comboLogType = new ComboBox { Width = 220, MinHeight = 32 };
            comboLogType.Items.Add(LanguageManager.Get("Settings.JSON"));
            comboLogType.Items.Add(LanguageManager.Get("Settings.XML"));
            comboLogType.SelectedIndex = _settings.LogFileType == "JSON" ? 0 : 1;
            comboLogType.SelectionChanged += ComboLogType_Changed;
            AddSettingRow(logsStack, LanguageManager.Get("Settings.FileType"), LanguageManager.Get("Settings.FileTypeDescription"), comboLogType, false);

            logsCard.Child = logsStack;
            content.Children.Add(logsCard);

            scrollViewer.Content = content;
            SetContent(scrollViewer);
            // AddConsoleLog("Navigation vers Parametres.", "info");
        }

        private void AddSectionHeader(StackPanel parent, string text)
        {
            var header = new TextBlock
            {
                Text = text,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            header.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            parent.Children.Add(header);
        }

        private void AddSettingRow(StackPanel parent, string label, string description, UIElement control, bool hasSeparator = true)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, hasSeparator ? 14 : 0) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var textStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var labelBlock = new TextBlock
            {
                Text = label,
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            labelBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            textStack.Children.Add(labelBlock);

            var descBlock = new TextBlock
            {
                Text = description,
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 0),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            descBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            textStack.Children.Add(descBlock);

            Grid.SetColumn(textStack, 0);
            row.Children.Add(textStack);

            var controlWrapper = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            controlWrapper.Children.Add(control);
            Grid.SetColumn(controlWrapper, 1);
            row.Children.Add(controlWrapper);

            parent.Children.Add(row);

            if (hasSeparator)
            {
                var sep = new Border
                {
                    Height = 1,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                sep.SetResourceReference(Border.BackgroundProperty, "BorderSubtleBrush");
                parent.Children.Add(sep);
            }
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
        
        private void BtnCreateWork_Click()
        {
            var dialog = new CreateWorkDialog()
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                var backupRequest = new BackupCreateRequest
                {
                    Name = dialog.WorkName,
                    SourceFilePath = dialog.SourcePath,
                    DestinationFilePath = dialog.DestinationPath,
                    Type = dialog.SaveType == "Complete"
                        ? BackupType.Full
                        : BackupType.Sequential
                };
                
                _vm.BackupCreateRequest = backupRequest;
                _vm.CreateBackupCommand.Execute(null);
            }
        }
        

        private void BtnExecuteWorks_Click(object sender, RoutedEventArgs e)
        {
            // AddConsoleLog("Execution des travaux de sauvegarde...", "info");
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

            // AddConsoleLog($"Template change: {_settings.AppTemplate}", "info");
        }

        private void ComboTheme_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            var combo = (ComboBox)sender;
            _settings.AppTheme = combo.SelectedIndex == 0 ? "Light" : "Dark";
            SettingsManager.Save(_settings);
            ThemeManager.ApplyTheme(_settings.AppTheme);

            // AddConsoleLog($"Theme change: {_settings.AppTheme}", "success");
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

            // AddConsoleLog($"Mode d'execution: {(_settings.AutoExecute ? "Auto" : "Manuel")}", "info");
        }

        private void ComboLogType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            var combo = (ComboBox)sender;
            _settings.LogFileType = combo.SelectedIndex == 0 ? "JSON" : "XML";
            SettingsManager.Save(_settings);

            // AddConsoleLog($"Format de log: {_settings.LogFileType}", "info");
        }

        #endregion
    }
}
