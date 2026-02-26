using Microsoft.Win32;
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
            _vm.MaxBandwidthKbps = _settings.MaxBandwidthKbps;
            _vm.LogsDirectory    = _settings.LogsDirectory;
            _vm.LogFileType      = _settings.LogFileType;

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

            SetContent(content);
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

            bool allSelected = false;
            var btnSelectAll = new Button
            {
                Content = LanguageManager.Get("Saves.SelectAll"),
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnPanel.Children.Add(btnSelectAll);

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

            var checkboxes = new List<(Backup backup, CheckBox cb)>();

            void AddCard(Backup b)
            {
                var (card, cb) = BuildSaveCard(b);
                checkboxes.Add((b, cb));
                content.Children.Add(card);
            }

            foreach (var backup in _vm.Backups.OrderByDescending(b => b.IsPriority))
                AddCard(backup);

            btnSelectAll.Click += (s, e) =>
            {
                allSelected = !allSelected;
                foreach (var (b, cb) in checkboxes)
                {
                    b.IsSelected = allSelected;
                    cb.IsChecked = allSelected;
                }
                btnSelectAll.Content = allSelected
                    ? LanguageManager.Get("Saves.DeselectAll")
                    : LanguageManager.Get("Saves.SelectAll");
            };

            _vm.Backups.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                    foreach (Backup newBackup in e.NewItems)
                        AddCard(newBackup);
            };

            SetContent(content);
        }

        private (Border card, CheckBox checkBox) BuildSaveCard(Backup backup)
        {
            var card = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14, 12, 14, 12),
                Margin = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(1)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            UpdateCardBorder(card, backup.IsPriority);

            var outer = new StackPanel();

            // ─── Main row ─────────────────────────────────────────────
            var mainRow = new Grid();
            mainRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // ── LEFT ────────────────────────────────────────────────
            var left = new StackPanel { Margin = new Thickness(0, 0, 16, 0) };

            // Row 1: [☐] [★] Name
            var nameRow = new Grid();
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // checkbox
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // star
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // name

            var checkBox = new CheckBox
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
                IsChecked = backup.IsSelected
            };
            checkBox.Checked   += (s, e) => backup.IsSelected = true;
            checkBox.Unchecked += (s, e) => backup.IsSelected = false;

            var starBtn = new Button
            {
                Content = backup.IsPriority ? "★" : "☆",
                Width = 22,
                Height = 22,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0),
                FontSize = 14,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent
            };
            starBtn.Foreground = backup.IsPriority
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"))
                : (Brush)FindResource("ForegroundSecondaryBrush");

            var nameBlock = new TextBlock
            {
                Text = backup.Name,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            nameBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");

            Grid.SetColumn(checkBox,   0);
            Grid.SetColumn(starBtn,    1);
            Grid.SetColumn(nameBlock,  2);
            nameRow.Children.Add(checkBox);
            nameRow.Children.Add(starBtn);
            nameRow.Children.Add(nameBlock);
            left.Children.Add(nameRow);

            // Row 2: [StatusBadge] [TypeBadge]  ← même hauteur
            var statusColor = GetStatusColor(backup.Status);
            var statusBadge = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
                Background = new SolidColorBrush(Color.FromArgb(30, statusColor.R, statusColor.G, statusColor.B))
            };
            var statusLabel = new TextBlock
            {
                Text = GetStatusText(backup.Status),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(statusColor),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            statusBadge.Child = statusLabel;

            bool isFull = backup.Type == BackupType.Full;
            var typeColor = isFull
                ? (Color)ColorConverter.ConvertFromString("#60A5FA")
                : (Color)ColorConverter.ConvertFromString("#A78BFA");
            var typeBadge = new Border
            {
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(6, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(25, typeColor.R, typeColor.G, typeColor.B))
            };
            typeBadge.Child = new TextBlock
            {
                Text = isFull ? LanguageManager.Get("CreateWork.Complete") : LanguageManager.Get("CreateWork.Incremental"),
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(typeColor),
                FontFamily = (FontFamily)FindResource("AppFont")
            };

            var badgeRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(50, 4, 0, 5)
            };
            badgeRow.Children.Add(statusBadge);
            badgeRow.Children.Add(typeBadge);
            left.Children.Add(badgeRow);

            // Paths
            var srcBlock = new TextBlock
            {
                Text = $"src: {SmartTruncatePath(backup.SourceFilePath)}",
                FontSize = 11,
                Margin = new Thickness(50, 0, 0, 2),
                FontFamily = (FontFamily)FindResource("MonoFont"),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            srcBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");

            var dstBlock = new TextBlock
            {
                Text = $"dst: {SmartTruncatePath(backup.DestinationFilePath)}",
                FontSize = 11,
                Margin = new Thickness(50, 0, 0, 0),
                FontFamily = (FontFamily)FindResource("MonoFont"),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            dstBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");

            left.Children.Add(srcBlock);
            left.Children.Add(dstBlock);
            Grid.SetColumn(left, 0);
            mainRow.Children.Add(left);

            // ── RIGHT ───────────────────────────────────────────────
            var right = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            var dateBlock = new TextBlock
            {
                Text = backup.LastBackupDateTime == default
                    ? "-"
                    : backup.LastBackupDateTime.ToString("dd/MM/yyyy  HH:mm"),
                FontSize = 11,
                TextAlignment = TextAlignment.Right,
                FontFamily = (FontFamily)FindResource("MonoFont")
            };
            dateBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            right.Children.Add(dateBlock);

            var sizeBlock = new TextBlock
            {
                Text = FormatSize(GetPathSize(backup.SourceFilePath)),
                FontSize = 11,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 2, 0, 8),
                FontFamily = (FontFamily)FindResource("MonoFont")
            };
            sizeBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            right.Children.Add(sizeBlock);

            // Actions: [▶/⏸] [•••]
            var actionsRow = new StackPanel { Orientation = Orientation.Horizontal };

            var playPauseBtn = new Button
            {
                Content = backup.Status == BackupStatus.InProgress ? "❚❚" : "▶",
                Width = 32,
                Height = 32,
                Padding = new Thickness(0),
                FontSize = 12,
                Margin = new Thickness(0, 0, 6, 0),
                BorderThickness = new Thickness(1),
                IsEnabled = backup.Status != BackupStatus.Completed
            };

            var moreBtn = new Button
            {
                Content = "•••",
                Width = 32,
                Height = 32,
                Padding = new Thickness(0),
                FontSize = 10,
                BorderThickness = new Thickness(1)
            };

            var ctxMenu = new ContextMenu();
            var menuStart  = new MenuItem { Header = LanguageManager.Get("Saves.Start") };
            var menuDelete = new MenuItem { Header = LanguageManager.Get("Saves.Delete") };
            ctxMenu.Items.Add(menuStart);
            ctxMenu.Items.Add(new Separator());
            ctxMenu.Items.Add(menuDelete);
            moreBtn.ContextMenu = ctxMenu;
            moreBtn.Click += (s, e) => moreBtn.ContextMenu.IsOpen = true;

            actionsRow.Children.Add(playPauseBtn);
            actionsRow.Children.Add(moreBtn);
            right.Children.Add(actionsRow);

            Grid.SetColumn(right, 1);
            mainRow.Children.Add(right);
            outer.Children.Add(mainRow);

            // ─── Progress bar row ──────────────────────────────────
            var progressRow = new Grid
            {
                Margin = new Thickness(0, 10, 0, 0),
                Visibility = (backup.Status == BackupStatus.InProgress || backup.Status == BackupStatus.Paused)
                    ? Visibility.Visible
                    : Visibility.Collapsed
            };
            progressRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            progressRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var progressBar = new ProgressBar { Value = backup.Progress, Maximum = 100, Height = 4 };
            Grid.SetColumn(progressBar, 0);

            var progressLabel = new TextBlock
            {
                Text = $"{backup.Progress:F0}%",
                FontSize = 10,
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            progressLabel.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            Grid.SetColumn(progressLabel, 1);

            progressRow.Children.Add(progressBar);
            progressRow.Children.Add(progressLabel);
            outer.Children.Add(progressRow);

            card.Child = outer;

            // ─── Event handlers ────────────────────────────────────
            playPauseBtn.Click += async (s, e) =>
            {
                if (backup.Status == BackupStatus.InProgress)
                {
                    _vm.ToggleBackup(backup);
                    UpdateStatusUI(statusBadge, statusLabel, playPauseBtn, progressRow, backup);
                }
                else if (backup.Status != BackupStatus.Completed)
                {
                    await _vm.ExecuteSingleJobAsync(backup, b => Dispatcher.InvokeAsync(() =>
                    {
                        progressBar.Value    = b.Progress;
                        progressLabel.Text   = $"{b.Progress:F0}%";
                        UpdateStatusUI(statusBadge, statusLabel, playPauseBtn, progressRow, b);
                    }));
                }
            };

            menuStart.Click += async (s, e) =>
            {
                if (backup.Status != BackupStatus.Completed)
                {
                    await _vm.ExecuteSingleJobAsync(backup, b => Dispatcher.InvokeAsync(() =>
                    {
                        progressBar.Value    = b.Progress;
                        progressLabel.Text   = $"{b.Progress:F0}%";
                        UpdateStatusUI(statusBadge, statusLabel, playPauseBtn, progressRow, b);
                    }));
                }
            };

            menuDelete.Click += (s, e) =>
            {
                _vm.DeleteBackup(backup);
                if (card.Parent is Panel parentPanel)
                    parentPanel.Children.Remove(card);
            };

            starBtn.Click += (s, e) =>
            {
                backup.IsPriority = !backup.IsPriority;
                _vm.UpdateBackup(backup);
                starBtn.Content = backup.IsPriority ? "★" : "☆";
                starBtn.Foreground = backup.IsPriority
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"))
                    : (Brush)FindResource("ForegroundSecondaryBrush");
                UpdateCardBorder(card, backup.IsPriority);
            };

            void onBatchUpdate(Backup b)
            {
                if (b.Id != backup.Id) return;
                Dispatcher.InvokeAsync(() =>
                {
                    progressBar.Value  = b.Progress;
                    progressLabel.Text = $"{b.Progress:F0}%";
                    UpdateStatusUI(statusBadge, statusLabel, playPauseBtn, progressRow, b);
                });
            }
            _vm.BackupUpdated += onBatchUpdate;
            card.Unloaded += (_, _) => _vm.BackupUpdated -= onBatchUpdate;

            return (card, checkBox);
        }

        private void UpdateStatusUI(Border statusBadge, TextBlock statusLabel, Button playPauseBtn, Grid progressRow, Backup backup)
        {
            var statusColor = GetStatusColor(backup.Status);
            statusLabel.Text = GetStatusText(backup.Status);
            statusLabel.Foreground = new SolidColorBrush(statusColor);
            statusBadge.Background = new SolidColorBrush(Color.FromArgb(30, statusColor.R, statusColor.G, statusColor.B));
            playPauseBtn.Content = backup.Status == BackupStatus.InProgress ? "❚❚" : "▶";
            playPauseBtn.IsEnabled = backup.Status != BackupStatus.Completed;
            progressRow.Visibility = (backup.Status == BackupStatus.InProgress || backup.Status == BackupStatus.Paused)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void UpdateCardBorder(Border card, bool isPriority)
        {
            card.SetResourceReference(Border.BorderBrushProperty, isPriority ? "AccentBrush" : "BorderBrush");
        }

        private string GetStatusText(BackupStatus status) => status switch
        {
            BackupStatus.Pending    => LanguageManager.Get("Saves.Pending"),
            BackupStatus.InProgress => LanguageManager.Get("Saves.InProgress"),
            BackupStatus.Paused     => LanguageManager.Get("Saves.Paused"),
            BackupStatus.Error      => LanguageManager.Get("Saves.Error"),
            BackupStatus.Completed  => LanguageManager.Get("Saves.Completed"),
            _                       => "?"
        };

        private Color GetStatusColor(BackupStatus status) => status switch
        {
            BackupStatus.Pending    => (Color)ColorConverter.ConvertFromString("#9CA3AF"),
            BackupStatus.InProgress => (Color)ColorConverter.ConvertFromString("#60A5FA"),
            BackupStatus.Paused     => (Color)ColorConverter.ConvertFromString("#FBBF24"),
            BackupStatus.Error      => (Color)ColorConverter.ConvertFromString("#F87171"),
            BackupStatus.Completed  => (Color)ColorConverter.ConvertFromString("#34D399"),
            _                       => (Color)ColorConverter.ConvertFromString("#9CA3AF")
        };

        private static string SmartTruncatePath(string path, int maxLen = 48)
        {
            if (path.Length <= maxLen) return path;

            string root = Path.GetPathRoot(path) ?? string.Empty;
            string name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (string.IsNullOrEmpty(name))
                name = new DirectoryInfo(path).Name;

            string attempt = $"{root}…{Path.DirectorySeparatorChar}{name}";
            if (attempt.Length <= maxLen) return attempt;

            // Le nom seul est trop long : on tronque en gardant l'extension
            string ext     = Path.GetExtension(name);
            string baseName = Path.GetFileNameWithoutExtension(name);
            int available  = maxLen - root.Length - 3 - ext.Length; // 3 = "…\" + "…"
            if (available > 1)
                return $"{root}…{Path.DirectorySeparatorChar}{baseName[..available]}…{ext}";

            return path[..maxLen] + "…";
        }

        private static long GetPathSize(string path)
        {
            try
            {
                if (File.Exists(path))
                    return new FileInfo(path).Length;
                if (Directory.Exists(path))
                    return new DirectoryInfo(path).GetFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
            }
            catch { }
            return 0;
        }

        private static string FormatSize(long bytes)
        {
            if (bytes <= 0) return "-";
            if (bytes < 1024) return $"{bytes} o";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} Ko";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} Mo";
            return $"{bytes / (1024.0 * 1024 * 1024):F1} Go";
        }

        private static string FormatDuration(long ms)
        {
            if (ms < 1000) return $"{ms} ms";
            if (ms < 60_000) return $"{ms / 1000.0:F1} s";
            long minutes = ms / 60_000;
            long seconds = (ms % 60_000) / 1000;
            return $"{minutes} m {seconds:D2} s";
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

            var subtitle = new TextBlock
            {
                Text = LanguageManager.Get("History.Subtitle"),
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 24),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            subtitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            content.Children.Add(subtitle);

            var entries = _vm.GetHistory();

            if (entries.Count == 0)
            {
                var emptyBorder = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(24, 32, 24, 32),
                    BorderThickness = new Thickness(1)
                };
                emptyBorder.SetResourceReference(Border.BackgroundProperty, "CardBrush");
                emptyBorder.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

                var emptyText = new TextBlock
                {
                    Text = LanguageManager.Get("History.Empty"),
                    FontSize = 13,
                    TextAlignment = TextAlignment.Center,
                    FontFamily = (FontFamily)FindResource("AppFont")
                };
                emptyText.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
                emptyBorder.Child = emptyText;
                content.Children.Add(emptyBorder);
            }
            else
            {
                foreach (var entry in entries)
                    AddHistoryItem(content, entry);
            }

            SetContent(content);
        }

        private void AddHistoryItem(StackPanel parent, EasySave.Model.HistoryEntry entry)
        {
            string statusText;
            Color statusColor;
            if (entry.Status == BackupStatus.Error)
            {
                statusText = LanguageManager.Get("History.Error");
                statusColor = (Color)ColorConverter.ConvertFromString("#F87171");
            }
            else if (entry.HasWarnings)
            {
                statusText = LanguageManager.Get("History.Warning");
                statusColor = (Color)ColorConverter.ConvertFromString("#FBBF24");
            }
            else
            {
                statusText = LanguageManager.Get("History.Completed");
                statusColor = (Color)ColorConverter.ConvertFromString("#34D399");
            }

            var card = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 11, 14, 11),
                Margin = new Thickness(0, 0, 0, 6),
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(60, statusColor.R, statusColor.G, statusColor.B))
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");

            var outer = new StackPanel();

            // ── Grid: 3 cols × 2 rows ────────────────────────────────
            // Col 0 (*) : name / src path   → always gets remaining space
            // Col 1 (Auto): date             → natural width, never forces clip
            // Col 2 (Auto): status badge     → natural width, always visible
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 60 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // row 0: name / date / status
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // row 1: src path / metrics

            // ── Row 0, Col 0 : backup name ──────────────────────────
            var nameBlock = new TextBlock
            {
                Text = entry.BackupName,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("AppFont"),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            nameBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            Grid.SetRow(nameBlock, 0);
            Grid.SetColumn(nameBlock, 0);
            grid.Children.Add(nameBlock);

            // ── Row 0, Col 1 : date ─────────────────────────────────
            var dateBlock = new TextBlock
            {
                Text = entry.StartTime.ToString("dd/MM/yyyy  HH:mm"),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 10, 0),
                FontFamily = (FontFamily)FindResource("MonoFont")
            };
            dateBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            Grid.SetRow(dateBlock, 0);
            Grid.SetColumn(dateBlock, 1);
            grid.Children.Add(dateBlock);

            // ── Row 0, Col 2 : status badge ─────────────────────────
            var statusBadge = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 3, 8, 3),
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(30, statusColor.R, statusColor.G, statusColor.B))
            };
            statusBadge.Child = new TextBlock
            {
                Text = statusText,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(statusColor),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            Grid.SetRow(statusBadge, 0);
            Grid.SetColumn(statusBadge, 2);
            grid.Children.Add(statusBadge);

            // ── Row 1, Col 0 : source path ──────────────────────────
            var srcBlock = new TextBlock
            {
                Text = $"src: {SmartTruncatePath(entry.SourcePath)}",
                FontSize = 10,
                Margin = new Thickness(0, 3, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("MonoFont"),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            srcBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            Grid.SetRow(srcBlock, 1);
            Grid.SetColumn(srcBlock, 0);
            grid.Children.Add(srcBlock);

            // ── Row 1, Col 1+2 span : files · size · duration ───────
            var metricsBlock = new TextBlock
            {
                Text = $"{entry.FileCount} {LanguageManager.Get("History.Files")}  ·  {FormatSize(entry.TotalSizeBytes)}  ·  {FormatDuration(entry.DurationMs)}",
                FontSize = 10,
                Margin = new Thickness(12, 3, 0, 0),
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = (FontFamily)FindResource("MonoFont")
            };
            metricsBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            Grid.SetRow(metricsBlock, 1);
            Grid.SetColumn(metricsBlock, 1);
            Grid.SetColumnSpan(metricsBlock, 2);
            grid.Children.Add(metricsBlock);

            outer.Children.Add(grid);

            // ── Error detail ─────────────────────────────────────────
            if (entry.Status == BackupStatus.Error && !string.IsNullOrWhiteSpace(entry.ErrorMessage))
            {
                outer.Children.Add(new TextBlock
                {
                    Text = $"{LanguageManager.Get("History.ErrorDetail")}{entry.ErrorMessage}",
                    FontSize = 10,
                    Margin = new Thickness(0, 6, 0, 0),
                    FontFamily = (FontFamily)FindResource("MonoFont"),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171"))
                });
            }

            card.Child = outer;
            parent.Children.Add(card);
        }

        private void SectionSettings()
        {
            UpdateNavStyles("settings");

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
            comboLanguage.SelectedIndex = _settings.Language == "French" ? 0 : 1;
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

            var tbBandwidth = new TextBox
            {
                Width = 220,
                MinHeight = 32,
                Text = _settings.MaxBandwidthKbps.ToString(),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            tbBandwidth.TextChanged += (s, e) =>
            {
                if (_isInitializing) return;
                if (int.TryParse(tbBandwidth.Text, out int val) && val >= 0)
                {
                    _settings.MaxBandwidthKbps = val;
                    SettingsManager.Save(_settings);
                    _vm.MaxBandwidthKbps = val;
                }
            };
            AddSettingRow(savesStack, LanguageManager.Get("Settings.BandwidthLimit"), LanguageManager.Get("Settings.BandwidthDescription"), tbBandwidth, false);

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

            var logsDirContainer = new StackPanel();

            var logsDirLabel = new TextBlock
            {
                Text = LanguageManager.Get("Settings.LogsFolder"),
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 2),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            logsDirLabel.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            logsDirContainer.Children.Add(logsDirLabel);

            var logsDirDesc = new TextBlock
            {
                Text = LanguageManager.Get("Settings.LogsFolderDescription"),
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 8),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            logsDirDesc.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            logsDirContainer.Children.Add(logsDirDesc);

            var inputGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var tbLogsDir = new TextBox
            {
                Height = 36,
                Text = _settings.LogsDirectory,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Thickness(8, 5, 8, 5),
                FontFamily = (FontFamily)FindResource("MonoFont")
            };
            Grid.SetColumn(tbLogsDir, 0);
            inputGrid.Children.Add(tbLogsDir);

            var btnBrowseLogs = new Button
            {
                Content = LanguageManager.Get("Settings.LogsFolderBrowse"),
                MinHeight = 32,
                Padding = new Thickness(12, 0, 12, 0),
                Margin = new Thickness(6, 0, 0, 0)
            };
            Grid.SetColumn(btnBrowseLogs, 1);
            inputGrid.Children.Add(btnBrowseLogs);
            logsDirContainer.Children.Add(inputGrid);

            // General path hint
            var pathHintBorder = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(1)
            };
            pathHintBorder.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            pathHintBorder.SetResourceReference(Border.BorderBrushProperty, "BorderSubtleBrush");
            var pathHintStack = new StackPanel();

            var pathHintText = new TextBlock
            {
                Text = LanguageManager.Get("Settings.LogsFolderHint"),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            pathHintText.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            pathHintStack.Children.Add(pathHintText);

            var btnLocalReset = new Button
            {
                Content = LanguageManager.Get("Settings.LocalQuickFill"),
                HorizontalAlignment = HorizontalAlignment.Left,
                MinHeight = 30,
                Margin = new Thickness(0, 8, 0, 0),
                Padding = new Thickness(12, 0, 12, 0)
            };
            pathHintStack.Children.Add(btnLocalReset);
            pathHintBorder.Child = pathHintStack;
            logsDirContainer.Children.Add(pathHintBorder);

            // Docker card
            var dockerCard = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 14),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(100, 14, 165, 233)),
                Background = new SolidColorBrush(Color.FromArgb(12, 14, 165, 233))
            };
            var dockerStack = new StackPanel();

            var dockerTitle = new TextBlock
            {
                Text = LanguageManager.Get("Settings.DockerTitle"),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 6),
                Foreground = new SolidColorBrush(Color.FromArgb(220, 14, 165, 233)),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            dockerStack.Children.Add(dockerTitle);

            var dockerHintText = new TextBlock
            {
                Text = LanguageManager.Get("Settings.DockerHint"),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18,
                Margin = new Thickness(0, 0, 0, 8),
                FontFamily = (FontFamily)FindResource("MonoFont")
            };
            dockerHintText.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            dockerStack.Children.Add(dockerHintText);

            var btnDockerPath = new Button
            {
                Content = LanguageManager.Get("Settings.DockerQuickFill"),
                HorizontalAlignment = HorizontalAlignment.Left,
                MinHeight = 30,
                Padding = new Thickness(12, 0, 12, 0),
                Foreground = new SolidColorBrush(Color.FromArgb(220, 14, 165, 233))
            };
            dockerStack.Children.Add(btnDockerPath);
            dockerCard.Child = dockerStack;
            logsDirContainer.Children.Add(dockerCard);

            btnBrowseLogs.Click += (s, e) =>
            {
                var dialog = new OpenFolderDialog { Title = LanguageManager.Get("Settings.LogsFolder") };
                if (dialog.ShowDialog() == true)
                {
                    tbLogsDir.Text = dialog.FolderName;
                    _settings.LogsDirectory = dialog.FolderName;
                    SettingsManager.Save(_settings);
                    _vm.LogsDirectory = _settings.LogsDirectory;
                }
            };
            tbLogsDir.TextChanged += (s, e) =>
            {
                if (_isInitializing) return;
                _settings.LogsDirectory = tbLogsDir.Text;
                SettingsManager.Save(_settings);
                _vm.LogsDirectory = _settings.LogsDirectory;
            };
            btnDockerPath.Click += (s, e) =>
            {
                tbLogsDir.Text = "/app/logs";
                _settings.LogsDirectory = "/app/logs";
                SettingsManager.Save(_settings);
                _vm.LogsDirectory = _settings.LogsDirectory;
            };
            btnLocalReset.Click += (s, e) =>
            {
                tbLogsDir.Text = "logs";
                _settings.LogsDirectory = "logs";
                SettingsManager.Save(_settings);
                _vm.LogsDirectory = "logs";
            };
            tbLogsDir.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tbLogsDir.Text))
                {
                    tbLogsDir.Text = "logs";
                    _settings.LogsDirectory = "logs";
                    SettingsManager.Save(_settings);
                    _vm.LogsDirectory = "logs";
                }
            };

            logsStack.Children.Add(logsDirContainer);

            var logsDirSep = new Border { Height = 1, Margin = new Thickness(0, 0, 0, 14) };
            logsDirSep.SetResourceReference(Border.BackgroundProperty, "BorderSubtleBrush");
            logsStack.Children.Add(logsDirSep);

            var comboLogType = new ComboBox { Width = 220, MinHeight = 32 };
            comboLogType.Items.Add(LanguageManager.Get("Settings.JSON"));
            comboLogType.Items.Add(LanguageManager.Get("Settings.XML"));
            comboLogType.SelectedIndex = _settings.LogFileType == "JSON" ? 0 : 1;
            comboLogType.SelectionChanged += ComboLogType_Changed;
            AddSettingRow(logsStack, LanguageManager.Get("Settings.FileType"), LanguageManager.Get("Settings.FileTypeDescription"), comboLogType, false);

            logsCard.Child = logsStack;
            content.Children.Add(logsCard);

            SetContent(content);
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
            _settings.Language = combo.SelectedIndex == 0 ? "French" : "English";
            SettingsManager.Save(_settings);
            LanguageManager.LoadLanguage(_settings.Language);

            UpdateUILanguage();

            if (ContentTemplate1.Content != null || ContentTemplate2.Content != null)
            {
                SectionSettings();
            }
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
