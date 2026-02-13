using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfSkeleton
{
    public partial class MainWindow : Window
    {
        private AppSettings _settings;
        private bool _isInitializing = true;
        private string _currentSection = "home";
        private List<WorkItemData> _workItems = new List<WorkItemData>();
        private Button _btnExecuteSelected;
        private Button _btnSelectAll;
        // Console removed - private bool _consoleCollapsed = false;

        public MainWindow()
        {
            InitializeComponent();

            _settings = SettingsManager.Load();

            LanguageManager.LoadLanguage(_settings.Language);
            ThemeManager.ApplyTheme(_settings.AppTheme);

            ApplyTemplate(_settings.AppTemplate);
            UpdateUILanguage();
            UpdateStorageInfo();
            SectionHome();

            // Console removed - Initial console logs
            // // AddConsoleLog("EasySave v.2 initialise.", "info");
            // // AddConsoleLog("Chargement de la configuration...", "info");
            // // AddConsoleLog($"Theme: {_settings.AppTheme} | Langue: {_settings.Language}", "info");
            // // AddConsoleLog("Application prete.", "success");

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

        /* Console removed - Implémentation console si jamais
        #region Console

        private void AddConsoleLog(string message, string type = "info")
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");

            Color color;
            string prefix;
            switch (type)
            {
                case "success":
                    color = (Color)ColorConverter.ConvertFromString("#34D399");
                    prefix = "OK";
                    break;
                case "warning":
                    color = (Color)ColorConverter.ConvertFromString("#FBBF24");
                    prefix = "WARN";
                    break;
                case "error":
                    color = (Color)ColorConverter.ConvertFromString("#F87171");
                    prefix = "ERR";
                    break;
                default:
                    color = (Color)ColorConverter.ConvertFromString("#60A5FA");
                    prefix = "INFO";
                    break;
            }

            var line = new TextBlock
            {
                FontFamily = (FontFamily)FindResource("MonoFont"),
                FontSize = 11.5,
                Margin = new Thickness(0, 1, 0, 1),
                TextWrapping = TextWrapping.Wrap
            };

            var tsRun = new Run($"[{timestamp}] ")
            {
                Foreground = (SolidColorBrush)FindResource("ForegroundSecondaryBrush")
            };
            line.Inlines.Add(tsRun);

            var prefixRun = new Run($"[{prefix}] ")
            {
                Foreground = new SolidColorBrush(color),
                FontWeight = FontWeights.SemiBold
            };
            line.Inlines.Add(prefixRun);

            var msgRun = new Run(message)
            {
                Foreground = (SolidColorBrush)FindResource("ForegroundBrush")
            };
            line.Inlines.Add(msgRun);

            ConsoleOutput.Children.Add(line);

            var line2 = new TextBlock
            {
                FontFamily = line.FontFamily,
                FontSize = line.FontSize,
                Margin = line.Margin,
                TextWrapping = TextWrapping.Wrap
            };
            line2.Inlines.Add(new Run($"[{timestamp}] ") { Foreground = tsRun.Foreground });
            line2.Inlines.Add(new Run($"[{prefix}] ") { Foreground = prefixRun.Foreground, FontWeight = FontWeights.SemiBold });
            line2.Inlines.Add(new Run(message) { Foreground = msgRun.Foreground });
            ConsoleOutput2.Children.Add(line2);

            ConsoleScroller.ScrollToEnd();
            ConsoleScroller2.ScrollToEnd();
        }

        private void BtnConsoleClear_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Children.Clear();
            ConsoleOutput2.Children.Clear();
        }

        private void BtnConsoleToggle_Click(object sender, RoutedEventArgs e)
        {
            _consoleCollapsed = !_consoleCollapsed;

            if (_consoleCollapsed)
            {
                ConsoleRow.Height = new GridLength(28);
                ConsoleRow2.Height = new GridLength(28);
                BtnConsoleToggle.Content = "\u25B2";
                BtnConsoleToggle2.Content = "\u25B2";
            }
            else
            {
                ConsoleRow.Height = new GridLength(180);
                ConsoleRow2.Height = new GridLength(180);
                BtnConsoleToggle.Content = "\u25BC";
                BtnConsoleToggle2.Content = "\u25BC";
            }
        }

        #endregion
        */

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

            AddStatCard(statsGrid, 0, "5", LanguageManager.Get("Home.ActiveJobs"), "#34D399");
            AddStatCard(statsGrid, 2, "128", LanguageManager.Get("Home.Completed"), "#60A5FA");
            AddStatCard(statsGrid, 4, "2", LanguageManager.Get("Home.Pending"), "#FBBF24");
            AddStatCard(statsGrid, 6, "0", LanguageManager.Get("Home.Errors"), "#F87171");

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
            AddRecentItem(content, "Sauvegarde Documents", "Termine - il y a 2h", "success");
            AddRecentItem(content, "Sauvegarde Projet Dev", "En cours - 67%", "info");
            AddRecentItem(content, "Sauvegarde Photos", "Planifie - 18:00", "warning");

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

            _workItems.Clear();
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
            btnCreateWork.Click += BtnCreateWork_Click;
            btnPanel.Children.Add(btnCreateWork);

            var btnExecuteWorks = new Button
            {
                Content = LanguageManager.Get("Saves.ExecuteWorks"),
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnExecuteWorks.Click += BtnExecuteWorks_Click;
            btnPanel.Children.Add(btnExecuteWorks);

            // Nouveau bouton pour exécuter les travaux sélectionnés
            _btnExecuteSelected = new Button
            {
                Content = LanguageManager.Get("Saves.ExecuteSelected"),
                Margin = new Thickness(0, 0, 10, 0),
                IsEnabled = false
            };
            _btnExecuteSelected.Click += BtnExecuteSelected_Click;
            btnPanel.Children.Add(_btnExecuteSelected);

            content.Children.Add(btnPanel);

            // Header avec "Works list" et bouton sélectionner tout
            var headerGrid = new Grid { Margin = new Thickness(0, 10, 0, 10) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var listTitle = new TextBlock
            {
                Text = LanguageManager.Get("Saves.WorksList"),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            listTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            Grid.SetColumn(listTitle, 0);
            headerGrid.Children.Add(listTitle);

            // Bouton pour tout sélectionner/désélectionner
            _btnSelectAll = new Button
            {
                Content = LanguageManager.Get("Saves.SelectAll"),
                Padding = new Thickness(12, 6, 12, 6),
                FontSize = 12
            };
            _btnSelectAll.Click += BtnSelectAll_Click;
            Grid.SetColumn(_btnSelectAll, 1);
            headerGrid.Children.Add(_btnSelectAll);

            content.Children.Add(headerGrid);

            // Works list as styled cards with checkboxes
            AddWorkItem(content, $"{LanguageManager.Get("Saves.Work")} 1 - Documents", LanguageManager.Get("Saves.Pending"), "warning", 0);
            AddWorkItem(content, $"{LanguageManager.Get("Saves.Work")} 2 - Projet Dev", LanguageManager.Get("Saves.InProgress"), "info", 67);
            AddWorkItem(content, $"{LanguageManager.Get("Saves.Work")} 3 - Photos", LanguageManager.Get("Saves.Completed"), "success", 100);

            SetContent(content);
            // AddConsoleLog("Navigation vers Sauvegardes.", "info");
        }

        private void AddWorkItem(StackPanel parent, string name, string status, string type, int progress)
        {
            var workData = new WorkItemData
            {
                Name = name,
                Status = status,
                Type = type,
                Progress = progress,
                IsSelected = false
            };

            var card = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(1)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Checkbox
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) }); // Spacing
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Content
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Actions

            // Checkbox
            var checkbox = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 0, 0)
            };
            checkbox.Checked += (s, e) => { workData.IsSelected = true; UpdateExecuteSelectedButton(); };
            checkbox.Unchecked += (s, e) => { workData.IsSelected = false; UpdateExecuteSelectedButton(); };
            workData.CheckBox = checkbox;
            Grid.SetColumn(checkbox, 0);
            mainGrid.Children.Add(checkbox);

            // Content stack
            var stack = new StackPanel();
            Grid.SetColumn(stack, 2);

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

            mainGrid.Children.Add(stack);

            // Actions panel (Play button + Menu button)
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 0, 0, 0)
            };
            Grid.SetColumn(actionsPanel, 3);

            // Play button avec icône compatible
            var btnPlay = new Button
            {
                Width = 32,
                Height = 32,
                Padding = new Thickness(0),
                ToolTip = "Exécuter ce travail",
                Margin = new Thickness(0, 0, 4, 0),
                Style = (Style)FindResource("PlayButton")
            };
            
            // Utiliser un TextBlock avec Segoe UI Symbol pour meilleure compatibilité
            var playIcon = new TextBlock
            {
                Text = "\uE102", // Play icon from Segoe MDL2 Assets
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            btnPlay.Content = playIcon;
            btnPlay.Click += (s, e) => ExecuteWork(workData);
            actionsPanel.Children.Add(btnPlay);

            // Menu button
            var btnMenu = new Button
            {
                Content = "•••",
                Width = 32,
                Height = 32,
                Padding = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                ToolTip = "Options",
                Style = (Style)FindResource("ActionButton")
            };
            btnMenu.Click += (s, e) => ShowWorkMenu(btnMenu, workData, parent, card);
            actionsPanel.Children.Add(btnMenu);

            mainGrid.Children.Add(actionsPanel);

            card.Child = mainGrid;
            parent.Children.Add(card);
            _workItems.Add(workData);
        }

        private void UpdateExecuteSelectedButton()
        {
            if (_btnExecuteSelected != null)
            {
                _btnExecuteSelected.IsEnabled = _workItems.Any(w => w.IsSelected);
            }
        }

        private void ExecuteWork(WorkItemData work)
        {
            // TODO: Implémenter l'exécution du travail
            // Pour le moment, rien ne se passe (front-end only)
        }

        private void ShowWorkMenu(Button menuButton, WorkItemData work, StackPanel parent, Border card)
        {
            var menu = new ContextMenu();
            
            var deleteItem = new MenuItem
            {
                Header = LanguageManager.Get("Saves.Delete"),
                Style = (Style)FindResource("DeleteMenuItem")
            };
            deleteItem.Click += (s, e) =>
            {
                // Suppression directe sans confirmation
                parent.Children.Remove(card);
                _workItems.Remove(work);
                UpdateExecuteSelectedButton();
                UpdateSelectAllButton();
            };
            menu.Items.Add(deleteItem);

            menu.PlacementTarget = menuButton;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            bool allSelected = _workItems.All(w => w.IsSelected);
            
            foreach (var work in _workItems)
            {
                work.IsSelected = !allSelected;
                if (work.CheckBox != null)
                {
                    work.CheckBox.IsChecked = work.IsSelected;
                }
            }

            UpdateSelectAllButton();
            UpdateExecuteSelectedButton();
        }

        private void UpdateSelectAllButton()
        {
            if (_btnSelectAll != null && _workItems.Any())
            {
                bool allSelected = _workItems.All(w => w.IsSelected);
                _btnSelectAll.Content = allSelected 
                    ? LanguageManager.Get("Saves.DeselectAll") 
                    : LanguageManager.Get("Saves.SelectAll");
            }
        }

        private void BtnExecuteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedWorks = _workItems.Where(w => w.IsSelected).ToList();
            if (selectedWorks.Any())
            {
                // TODO: Implémenter l'exécution des travaux sélectionnés
                // Pour le moment, rien ne se passe (front-end only)
            }
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
            AddHistoryItem(content, "2024-01-15 10:30", $"{LanguageManager.Get("Saves.Work")} 1", LanguageManager.Get("History.Success"), "success", "1.2 Go", "2m 34s");
            AddHistoryItem(content, "2024-01-15 09:15", $"{LanguageManager.Get("Saves.Work")} 2", LanguageManager.Get("History.Success"), "success", "856 Mo", "1m 12s");
            AddHistoryItem(content, "2024-01-14 16:45", $"{LanguageManager.Get("Saves.Work")} 3", LanguageManager.Get("History.Fail"), "error", "0 Mo", "0m 45s");
            AddHistoryItem(content, "2024-01-14 14:20", $"{LanguageManager.Get("Saves.Work")} 1", LanguageManager.Get("History.Abort"), "warning", "340 Mo", "0m 58s");
            AddHistoryItem(content, "2024-01-13 11:30", $"{LanguageManager.Get("Saves.Work")} 2", LanguageManager.Get("History.Success"), "success", "856 Mo", "1m 08s");

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

        private void BtnCreateWork_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateWorkDialog
            {
                Owner = this
            };

            // AddConsoleLog("Ouverture du dialogue de creation...", "info");

            if (dialog.ShowDialog() == true)
            {
                // TODO: Ajouter le travail créé à la liste
                // Pour le moment, rien ne se passe (front-end only)
                // AddConsoleLog($"Travail cree: {dialog.WorkName} ({dialog.SaveType})", "success");
            }
            else
            {
                // AddConsoleLog("Creation annulee.", "warning");
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

            ApplyTemplate(_settings.AppTemplate);

            // Recharger la section actuelle après le changement de template
            switch (_currentSection)
            {
                case "home": SectionHome(); break;
                case "saves": SectionSaves(); break;
                case "history": SectionHistory(); break;
                case "settings": SectionSettings(); break;
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
            _settings.Language = combo.SelectedIndex == 0 ? "Fran\u00e7ais" : "English";
            SettingsManager.Save(_settings);
            LanguageManager.LoadLanguage(_settings.Language);

            UpdateUILanguage();

            if (ContentTemplate1.Content != null || ContentTemplate2.Content != null)
            {
                SectionSettings();
            }

            // AddConsoleLog($"Langue changee: {_settings.Language}", "success");
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

    // Classe pour représenter un travail avec son état de sélection
    internal class WorkItemData
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public int Progress { get; set; }
        public bool IsSelected { get; set; }
        public CheckBox CheckBox { get; set; }
    }
}
