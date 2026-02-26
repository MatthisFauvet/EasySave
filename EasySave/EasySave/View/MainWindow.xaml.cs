using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using EasySave.Model;
using EasySave.View.Dialog;
using EasySave.ViewModel;
using CryptoSoft;

namespace EasySave.View
{
    public partial class MainWindow : Window
    {
        private AppSettings _settings;
        private bool _isInitializing = true;
        private string _currentSection = "home";
        private MainViewModel _vm;
        private List<CheckBox> _securityCheckboxes = new List<CheckBox>();
        private Button _encryptedButton, _decryptedButton;
        private readonly HashSet<Backup> _selectedBackups = new();
        
        private System.Collections.Specialized.NotifyCollectionChangedEventHandler? _backupsChangedHandler;

        public MainWindow()
        {
            _vm = new MainViewModel();
            DataContext = _vm;

            _vm.OpenCreateBackupDialogRequested += () => BtnCreateWork_Click();

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
            if (e.ClickCount == 2) BtnMaximize_Click(sender, e);
            else DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

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
                    long freeBytes  = systemDrive.AvailableFreeSpace;
                    long usedBytes  = totalBytes - freeBytes;

                    double usedGB         = usedBytes / (1024.0 * 1024.0 * 1024.0);
                    double totalGB        = totalBytes / (1024.0 * 1024.0 * 1024.0);
                    double percentageUsed = (usedBytes / (double)totalBytes) * 100;

                    StorageProgressBar.Value = percentageUsed;
                    StorageTextBlock.Text    = $"{usedGB:F1} Go / {totalGB:F1} Go";
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
            Title              = LanguageManager.Get("App.Title");
            TitleVersion.Text  = "  " + LanguageManager.Get("App.Version");

            NavTitle1.Text     = LanguageManager.Get("Navigation.Title");
            BtnHome1.Content   = LanguageManager.Get("Navigation.Home");
            BtnSaves1.Content  = LanguageManager.Get("Navigation.Saves");
            BtnHistory1.Content = LanguageManager.Get("Navigation.History");
            BtnSecurity1.Content = LanguageManager.Get("Navigation.Security");
            BtnSettings1.Content = LanguageManager.Get("Navigation.Settings");

            BtnHome2.Content   = LanguageManager.Get("Navigation.Home");
            BtnSaves2.Content  = LanguageManager.Get("Navigation.Saves");
            BtnHistory2.Content = LanguageManager.Get("Navigation.History");
            BtnSecurity2.Content = LanguageManager.Get("Navigation.Security");
            BtnSettings2.Content = LanguageManager.Get("Navigation.Settings");

            StorageTitleBlock.Text = LanguageManager.Get("Storage.Title");

            switch (_currentSection)
            {
                case "home":     SectionHome();     break;
                case "saves":    SectionSaves();    break;
                case "history":  SectionHistory();  break;
                case "settings": SectionSettings(); break;
            }
        }

        #endregion

        #region Navigation

        private void UpdateNavStyles(string section)
        {
            _currentSection = section;

            BtnHome1.Style     = (Style)FindResource(section == "home"     ? "NavButtonActive" : "NavButton");
            BtnSaves1.Style    = (Style)FindResource(section == "saves"    ? "NavButtonActive" : "NavButton");
            BtnHistory1.Style  = (Style)FindResource(section == "history"  ? "NavButtonActive" : "NavButton");
            BtnSettings1.Style = (Style)FindResource(section == "settings" ? "NavButtonActive" : "NavButton");

            BtnHome2.Style     = (Style)FindResource(section == "home"     ? "NavButtonActive" : "NavButton");
            BtnSaves2.Style    = (Style)FindResource(section == "saves"    ? "NavButtonActive" : "NavButton");
            BtnHistory2.Style  = (Style)FindResource(section == "history"  ? "NavButtonActive" : "NavButton");
            BtnSettings2.Style = (Style)FindResource(section == "settings" ? "NavButtonActive" : "NavButton");
        }

        private void NavHome_Click(object sender, RoutedEventArgs e)     => SectionHome();
        private void NavSaves_Click(object sender, RoutedEventArgs e)    => SectionSaves();
        private void NavHistory_Click(object sender, RoutedEventArgs e)
        {
            SectionHistory();
        }

        private void NavSecurity_Click(object sender, RoutedEventArgs e)
        {
            SectionSecurity();
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

            var content     = new StackPanel();
            var headerGrid  = new Grid { Margin = new Thickness(0, 0, 0, 28) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var headerStack = new StackPanel();
            var title = new TextBlock
            {
                Text       = LanguageManager.Get("Home.Title"),
                FontSize   = 26,
                FontWeight = FontWeights.Bold,
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            headerStack.Children.Add(title);

            var subtitle = new TextBlock
            {
                Text       = LanguageManager.Get("Home.Welcome"),
                FontSize   = 13,
                Margin     = new Thickness(0, 6, 0, 0),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            subtitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            headerStack.Children.Add(subtitle);
            headerGrid.Children.Add(headerStack);
            content.Children.Add(headerGrid);

            var statsGrid = new Grid { Margin = new Thickness(0, 0, 0, 24) };
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            AddStatCard(statsGrid, 0, _vm.TotalCount.ToString(), LanguageManager.Get("Home.AmountOfBackup"), "#34D399");

            content.Children.Add(statsGrid);

            var recentTitle = new TextBlock
            {
                Text       = LanguageManager.Get("Home.RecentActivity"),
                FontSize   = 15,
                FontWeight = FontWeights.SemiBold,
                Margin     = new Thickness(0, 0, 0, 12),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            recentTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(recentTitle);

            SetContent(content);
        }

        private void AddStatCard(Grid parent, int column, string value, string label, string colorHex)
        {
            var card = new Border
            {
                CornerRadius    = new CornerRadius(8),
                Padding         = new Thickness(16, 14, 16, 14),
                BorderThickness = new Thickness(1),
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var stack    = new StackPanel();
            var valBlock = new TextBlock
            {
                Text       = value,
                FontSize   = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            stack.Children.Add(valBlock);

            var labelBlock = new TextBlock
            {
                Text       = label,
                FontSize   = 11,
                Margin     = new Thickness(0, 4, 0, 0),
                FontFamily = (FontFamily)FindResource("AppFont"),
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
                CornerRadius    = new CornerRadius(6),
                Padding         = new Thickness(14, 10, 14, 10),
                Margin          = new Thickness(0, 0, 0, 6),
                BorderThickness = new Thickness(1),
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderSubtleBrush");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text                = name,
                FontSize            = 13,
                VerticalAlignment   = VerticalAlignment.Center,
                FontFamily          = (FontFamily)FindResource("AppFont"),
            };
            nameBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            Grid.SetColumn(nameBlock, 0);
            grid.Children.Add(nameBlock);

            Color statusColor = type switch
            {
                "success" => (Color)ColorConverter.ConvertFromString("#34D399"),
                "warning" => (Color)ColorConverter.ConvertFromString("#FBBF24"),
                "error"   => (Color)ColorConverter.ConvertFromString("#F87171"),
                _         => (Color)ColorConverter.ConvertFromString("#60A5FA"),
            };

            var statusBlock = new TextBlock
            {
                Text              = status,
                FontSize          = 11,
                Foreground        = new SolidColorBrush(statusColor),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily        = (FontFamily)FindResource("AppFont"),
            };
            Grid.SetColumn(statusBlock, 1);
            grid.Children.Add(statusBlock);

            card.Child = grid;
            parent.Children.Add(card);
        }

        private void SectionSaves()
        {
            UpdateNavStyles("saves");

            if (_backupsChangedHandler != null)
                _vm.BackupItems.CollectionChanged -= _backupsChangedHandler;

            var content = new StackPanel();
            content.DataContext = _vm;

            var title = new TextBlock
            {
                Text       = LanguageManager.Get("Saves.Title"),
                FontSize   = 26,
                FontWeight = FontWeights.Bold,
                Margin     = new Thickness(0, 0, 0, 6),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var subtitle = new TextBlock
            {
                Text       = LanguageManager.Get("Saves.Subtitle"),
                FontSize   = 13,
                Margin     = new Thickness(0, 0, 0, 24),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            subtitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            content.Children.Add(subtitle);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 20) };

            var btnCreateWork = new Button
            {
                Content = LanguageManager.Get("Saves.CreateWork"),
                Style   = (Style)FindResource("PrimaryButton"),
                Margin  = new Thickness(0, 0, 10, 0),
            };
            btnCreateWork.SetBinding(Button.CommandProperty, new Binding("OpenCreateBackupDialogCommand"));
            btnPanel.Children.Add(btnCreateWork);

            var btnExecuteWorks = new Button
            {
                Content = LanguageManager.Get("Saves.ExecuteWorks"),
                Margin  = new Thickness(0, 0, 10, 0),
            };
            btnExecuteWorks.SetBinding(Button.CommandProperty, new Binding("ExecuteBackupsCommand"));
            btnPanel.Children.Add(btnExecuteWorks);
            content.Children.Add(btnPanel);

            var statusText = new TextBlock
            {
                FontSize   = 12,
                Margin     = new Thickness(0, 6, 0, 0),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            statusText.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            statusText.SetBinding(TextBlock.TextProperty, new Binding("ExecutionStatus"));
            content.Children.Add(statusText);

            var listTitle = new TextBlock
            {
                Text       = LanguageManager.Get("Saves.WorksList"),
                FontSize   = 15,
                FontWeight = FontWeights.SemiBold,
                Margin     = new Thickness(0, 10, 0, 10),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            listTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(listTitle);

            var searchRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };

            var tbSearch = new TextBox { Width = 260, MinHeight = 32, Margin = new Thickness(0, 0, 10, 0) };
            tbSearch.SetBinding(TextBox.TextProperty, new Binding("SearchQuery")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            searchRow.Children.Add(tbSearch);

            var btnClear = new Button { Content = "Reset", MinHeight = 32 };
            btnClear.Click += (_, __) => _vm.SearchQuery = "";
            searchRow.Children.Add(btnClear);

            content.Children.Add(searchRow);

            // =========================
            // LIST PANEL
            // =========================
            var listPanel = new StackPanel();
            content.Children.Add(listPanel);

            foreach (var item in _vm.BackupItems)
                AddWorkItem(listPanel, item);

            _backupsChangedHandler = (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (BackupProgressItem newItem in e.NewItems!)
                            AddWorkItem(listPanel, newItem);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (BackupProgressItem oldItem in e.OldItems!)
                            RemoveWorkItem(listPanel, oldItem);
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        listPanel.Children.Clear();
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        foreach (BackupProgressItem oldItem in e.OldItems!)
                            RemoveWorkItem(listPanel, oldItem);
                        foreach (BackupProgressItem newItem in e.NewItems!)
                            AddWorkItem(listPanel, newItem);
                        break;
                }
            };

            _vm.BackupItems.CollectionChanged += _backupsChangedHandler;

            // =========================
            // PAGINATION BAR
            // =========================
            var pager = new StackPanel
            {
                Orientation         = Orientation.Horizontal,
                Margin              = new Thickness(0, 25, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var btnPrev = new Button
            {
                Content = "◀",
                Width = 40,
                Height = 32,
                Margin = new Thickness(0, 0, 8, 0),
                Padding = new Thickness(0),
                FontSize = 16,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            btnPrev.SetBinding(Button.CommandProperty, new Binding("PreviousPageCommand"));
            pager.Children.Add(btnPrev);

            var pageText = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontFamily = (FontFamily)FindResource("AppFont") };
            pageText.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            pageText.SetBinding(TextBlock.TextProperty, new Binding("PageIndex") { StringFormat = "Page {0}" });
            pager.Children.Add(pageText);

            var sep = new TextBlock { Text = " / ", VerticalAlignment = VerticalAlignment.Center, FontFamily = (FontFamily)FindResource("AppFont") };
            sep.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            pager.Children.Add(sep);

            var totalPagesText = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontFamily = (FontFamily)FindResource("AppFont") };
            totalPagesText.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            totalPagesText.SetBinding(TextBlock.TextProperty, new Binding("TotalPages"));
            pager.Children.Add(totalPagesText);

            var btnNext = new Button
            {
                Content = "▶",
                Width = 40,
                Height = 32,
                Margin = new Thickness(8, 0, 0, 0),
                Padding = new Thickness(0),
                FontSize = 16,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            btnNext.SetBinding(Button.CommandProperty, new Binding("NextPageCommand"));
            pager.Children.Add(btnNext);

            content.Children.Add(pager);
            SetContent(content);
        }

        private void RemoveWorkItem(StackPanel listPanel, BackupProgressItem item)
        {
            var control = listPanel.Children
                .Cast<UIElement>()
                .FirstOrDefault(c => c is FrameworkElement fe && fe.Tag == item);

            if (control != null)
                listPanel.Children.Remove(control);
        }

        private void AddWorkItem(StackPanel parent, BackupProgressItem item)
        {
            var backup = item.Backup;

            var card = new Border
            {
                CornerRadius    = new CornerRadius(8),
                Padding         = new Thickness(16, 12, 16, 12),
                Margin          = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(1),
                Tag             = item,
                DataContext     = item
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var stack = new StackPanel();

            var topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text              = backup.Name,
                FontSize          = 16,
                FontWeight        = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            nameBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            Grid.SetColumn(nameBlock, 0);
            topRow.Children.Add(nameBlock);

            Color badgeColor = backup.Type.ToString() switch
            {
                "Full"       => (Color)ColorConverter.ConvertFromString("#90D5FF"),
                "Sequential" => (Color)ColorConverter.ConvertFromString("#88E788"),
                _            => (Color)ColorConverter.ConvertFromString("#60A5FA")
            };
            var badge = new Border
            {
                CornerRadius      = new CornerRadius(10),
                Padding           = new Thickness(8, 3, 8, 3),
                Background        = new SolidColorBrush(badgeColor),
                Margin            = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            badge.Child = new TextBlock
            {
                Text       = backup.Type.ToString(),
                FontSize   = 14,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetColumn(badge, 1);
            topRow.Children.Add(badge);

            var pauseButton = BuildIconButton("⏸", margin: new Thickness(8, 0, 0, 0));
            var pauseLabel = (TextBlock)pauseButton.Content;
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(BackupProgressItem.IsPaused) or nameof(BackupProgressItem.Status))
                    pauseLabel.Text = item.IsPaused ? "▶" : "⏸";
            };
            pauseButton.Click += (_, _) =>
            {
                if (item.IsPaused) _vm.ResumeBackup(backup);
                else               _vm.PauseBackup(backup);
            };
            Grid.SetColumn(pauseButton, 2);
            topRow.Children.Add(pauseButton);

            var deleteButton = BuildIconButton("🗑", margin: new Thickness(8, 0, 0, 0));
            deleteButton.Click += (_, _) =>
            {
                _vm.RemoveBackup(backup);
                parent.Children.Remove(card);
            };
            Grid.SetColumn(deleteButton, 3);
            topRow.Children.Add(deleteButton);

            stack.Children.Add(topRow);

            var progressRow = new Grid { Margin = new Thickness(0, 10, 0, 0) };
            progressRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            progressRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var progressBar = BuildColorableProgressBar();
            progressBar.DataContext = item;
            progressBar.SetBinding(ProgressBar.ValueProperty, new Binding("Progress"));
            Grid.SetColumn(progressBar, 0);
            progressRow.Children.Add(progressBar);

            var progressLabel = new TextBlock
            {
                DataContext       = item,
                FontSize          = 11,
                Margin            = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily        = (FontFamily)FindResource("AppFont"),
            };
            progressLabel.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");

            var multiBinding = new MultiBinding { StringFormat = "{0} / {1} files  ({2}%)" };
            multiBinding.Bindings.Add(new Binding("FilesUploaded"));
            multiBinding.Bindings.Add(new Binding("TotalFiles"));
            multiBinding.Bindings.Add(new Binding("Progress"));
            progressLabel.SetBinding(TextBlock.TextProperty, multiBinding);

            Grid.SetColumn(progressLabel, 1);
            progressRow.Children.Add(progressLabel);

            stack.Children.Add(progressRow);

            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(BackupProgressItem.Status) &&
                    e.PropertyName != nameof(BackupProgressItem.Progress)) return;

                progressBar.Foreground = item.Status switch
                {
                    BackupStatus.Completed => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34D399")),
                    BackupStatus.Error     => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171")),
                    BackupStatus.Cancelled => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24")),
                    BackupStatus.Paused    => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A78BFA")),
                    _                      => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60A5FA")),
                };
            };

            card.Child = stack;
            parent.Children.Add(card);
        }

        private static ProgressBar BuildColorableProgressBar()
        {
            var indicatorFill = new FrameworkElementFactory(typeof(Rectangle));
            indicatorFill.SetValue(Rectangle.RadiusXProperty, 3.0);
            indicatorFill.SetValue(Rectangle.RadiusYProperty, 3.0);
            indicatorFill.SetBinding(
                Rectangle.FillProperty,
                new Binding("Foreground") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );

            var indicatorGrid = new FrameworkElementFactory(typeof(Grid));
            indicatorGrid.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            indicatorGrid.AppendChild(indicatorFill);

            var track = new FrameworkElementFactory(typeof(Border));
            track.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            track.SetValue(Border.ClipToBoundsProperty, true);
            track.SetBinding(
                Border.BackgroundProperty,
                new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );

            var indicator = new FrameworkElementFactory(typeof(Border));
            indicator.SetValue(FrameworkElement.NameProperty, "PART_Indicator");
            indicator.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            indicator.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            indicator.SetBinding(
                Border.BackgroundProperty,
                new Binding("Foreground") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );

            var trackBorder = new FrameworkElementFactory(typeof(Border));
            trackBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            trackBorder.SetValue(Border.ClipToBoundsProperty, true);
            trackBorder.SetBinding(
                Border.BackgroundProperty,
                new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );
            trackBorder.SetValue(FrameworkElement.NameProperty, "PART_Track");
            trackBorder.AppendChild(indicator);

            var template = new ControlTemplate(typeof(ProgressBar));
            template.VisualTree = trackBorder;

            return new ProgressBar
            {
                Template          = template,
                Background        = new SolidColorBrush(Color.FromArgb(40, 96, 165, 250)),
                Foreground        = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60A5FA")),
                Height            = 6,
                Minimum           = 0,
                Maximum           = 100,
                Value             = 0,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private Button BuildIconButton(string emoji, Thickness margin = default)
        {
            var btn = new Button
            {
                Width           = 30,
                Height          = 30,
                Margin          = margin,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor          = Cursors.Hand,
                Background      = Brushes.Transparent,
                BorderBrush     = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding         = new Thickness(0),
            };

            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentFactory.SetValue(VerticalAlignmentProperty,   VerticalAlignment.Center);
            btn.Template = new ControlTemplate(typeof(Button)) { VisualTree = contentFactory };

            btn.Content = new TextBlock
            {
                Text                = emoji,
                FontSize            = 16,
                VerticalAlignment   = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            return btn;
        }

        private void AddSecurityItemFromBackup(StackPanel content, Backup backup)
        {
            AddSecurityItem(content, backup);
        }

        private void AddSecurityItem(StackPanel parent, Backup backup)
        {
            var card = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(1),
                Tag = backup
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderSubtleBrush");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameBlock = new TextBlock
            {
                Text = backup.Name,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
            };
            nameBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            Grid.SetColumn(nameBlock, 0);
            grid.Children.Add(nameBlock);

            var cb = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };

            cb.Checked += (_, __) =>
            {
               _selectedBackups.Add(backup);
               _encryptedButton.IsEnabled  = _selectedBackups.Count > 0;
               _decryptedButton.IsEnabled = _selectedBackups.Count > 0;
            };

            cb.Unchecked += (_, __) =>
            {
                _selectedBackups.Remove(backup);
                _encryptedButton.IsEnabled = _selectedBackups.Count > 0;
            };

            Grid.SetColumn(cb, 1);
            grid.Children.Add(cb);

            card.Child = grid;
            parent.Children.Add(card);
        }
    
        private void SectionHistory()
        {
            UpdateNavStyles("history");

            var content = new StackPanel();

            var title = new TextBlock
            {
                Text       = LanguageManager.Get("History.Title"),
                FontSize   = 26,
                FontWeight = FontWeights.Bold,
                Margin     = new Thickness(0, 0, 0, 6),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var description = new TextBlock
            {
                Text       = LanguageManager.Get("History.Description"),
                FontSize   = 13,
                Margin     = new Thickness(0, 0, 0, 24),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            description.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            content.Children.Add(description);

            SetContent(content);
        }

        private void AddHistoryItem(StackPanel parent, string date, string work, string status, string type, string size, string duration)
        {
            var card = new Border
            {
                CornerRadius    = new CornerRadius(6),
                Padding         = new Thickness(14, 10, 14, 10),
                Margin          = new Thickness(0, 0, 0, 4),
                BorderThickness = new Thickness(1),
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderSubtleBrush");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            void AddCell(int col, string text, string fontKey = "AppFont", string fgKey = "ForegroundSecondaryBrush", int fontSize = 11)
            {
                var tb = new TextBlock { Text = text, FontSize = fontSize, VerticalAlignment = VerticalAlignment.Center, FontFamily = (FontFamily)FindResource(fontKey) };
                tb.SetResourceReference(TextBlock.ForegroundProperty, fgKey);
                Grid.SetColumn(tb, col);
                grid.Children.Add(tb);
            }

            AddCell(0, date, "MonoFont");
            AddCell(1, work, fontSize: 13, fgKey: "ForegroundBrush");
            AddCell(2, size, "MonoFont");
            AddCell(3, duration, "MonoFont");

            Color statusColor = type switch
            {
                "success" => (Color)ColorConverter.ConvertFromString("#34D399"),
                "warning" => (Color)ColorConverter.ConvertFromString("#FBBF24"),
                "error"   => (Color)ColorConverter.ConvertFromString("#F87171"),
                _         => (Color)ColorConverter.ConvertFromString("#60A5FA"),
            };
            var statusBadge = new Border
            {
                CornerRadius        = new CornerRadius(10),
                Padding             = new Thickness(8, 2, 8, 2),
                HorizontalAlignment = HorizontalAlignment.Right,
                Background          = new SolidColorBrush(Color.FromArgb(25, statusColor.R, statusColor.G, statusColor.B)),
            };
            statusBadge.Child = new TextBlock
            {
                Text       = status,
                FontSize   = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(statusColor),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            Grid.SetColumn(statusBadge, 4);
            grid.Children.Add(statusBadge);

            card.Child = grid;
            parent.Children.Add(card);
        }
        

        private void SectionSecurity()
        {
            UpdateNavStyles("security");
            var content = new StackPanel();
            var title = new TextBlock
            {
                Text = LanguageManager.Get("Security.Title"),
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 6),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);
            _securityCheckboxes.Clear();
            var listPanel = new StackPanel();
            content.Children.Add(listPanel);
            foreach (var b in _vm.Backups)
            {
               AddSecurityItemFromBackup(listPanel, b);
            }

            _encryptedButton = new Button
            {
                Content = LanguageManager.Get("Security.Encrypt"),
                Style = (Style)FindResource("PrimaryButton"),
                Margin = new Thickness(0, 20, 0, 0),
                IsEnabled = false
            };

            _decryptedButton = new Button
            {
                Content = LanguageManager.Get("Security.Decrypt"),
                Margin = new Thickness(0, 20, 0, 0),
                IsEnabled = false
            };

            _encryptedButton.Click += EncryptSelectedBackups;
            _decryptedButton.Click += DecryptSelectedBackups;
            content.Children.Add(_encryptedButton);
            content.Children.Add(_decryptedButton);
            SetContent(content);
        }

        private void SecurityCheckboxChanged(object sender, RoutedEventArgs e)
        {
            _encryptedButton.IsEnabled = _securityCheckboxes.Any(cb => cb.IsChecked == true);
        }

        private void EncryptSelectedBackups(object sender, RoutedEventArgs e)
        {
            string password = Microsoft.VisualBasic.Interaction.InputBox(
                "Entrez le mot de passe de chiffrement :",
                "Chiffrement",
                "",
                -1,
                -1
            );

            foreach (var backup in _selectedBackups)
            {
                var root = backup.DestinationFilePath;
                if (!Directory.Exists(root)) continue;

                var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (file.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                        continue;

                    try
                    {
                        var tmpEnc = file + ".enc";
                        CryptoSoft.Encrypter.EncryptFile(file, tmpEnc, password);
                        File.Delete(file);
                        File.Move(tmpEnc, file + ".enc");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur chiffrement: {file}\n{ex.Message}");
                    }
                }
            }

            MessageBox.Show(LanguageManager.Get("Security.Success"));
        }

        private void DecryptSelectedBackups(object sender, RoutedEventArgs e)
        {
            string password = Microsoft.VisualBasic.Interaction.InputBox(
                 "Entrez le mot de passe de chiffrement :",
                 "Chiffrement",
                 "",
                 -1,
                 -1
             );

            foreach (var backup in _selectedBackups)
            {
                string root = backup.DestinationFilePath;
                if (!Directory.Exists(root)) continue;

                var encryptedFiles = Directory.GetFiles(root, "*.enc", SearchOption.AllDirectories);

                foreach (var encFile in encryptedFiles)
                {
                    try
                    {
                        string outputFile = encFile.Substring(0, encFile.Length - 4);
                        CryptoSoft.Encrypter.DecryptFile(encFile, outputFile, password);
                        File.Delete(encFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur décryptage:\n{encFile}\n{ex.Message}");
                    }
                }
            }

            MessageBox.Show(LanguageManager.Get("Security.SuccessDecrypt"));
        }

        private void SectionSettings()
        {
            UpdateNavStyles("settings");

            var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var content      = new StackPanel();

            var title = new TextBlock
            {
                Text       = LanguageManager.Get("Settings.Title"),
                FontSize   = 26,
                FontWeight = FontWeights.Bold,
                Margin     = new Thickness(0, 0, 0, 6),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            title.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            content.Children.Add(title);

            var subtitle = new TextBlock
            {
                Text       = LanguageManager.Get("Settings.Subtitle"),
                FontSize   = 13,
                Margin     = new Thickness(0, 0, 0, 28),
                FontFamily = (FontFamily)FindResource("AppFont"),
            };
            subtitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            content.Children.Add(subtitle);

            // ── GENERAL ──────────────────────────────────────────────────────
            AddSectionHeader(content, LanguageManager.Get("Settings.General"));

            var generalCard = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(20, 16, 20, 16), Margin = new Thickness(0, 0, 0, 20), BorderThickness = new Thickness(1) };
            generalCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            generalCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var generalStack = new StackPanel();

            var comboTemplate = new ComboBox { Width = 220, MinHeight = 32 };
            comboTemplate.Items.Add(LanguageManager.Get("Settings.Template1"));
            comboTemplate.Items.Add(LanguageManager.Get("Settings.Template2"));
            comboTemplate.SelectedIndex     = _settings.AppTemplate - 1;
            comboTemplate.SelectionChanged += ComboTemplate_Changed;
            AddSettingRow(generalStack, LanguageManager.Get("Settings.TemplateApp"), LanguageManager.Get("Settings.TemplateDescription"), comboTemplate);

            var comboTheme = new ComboBox { Width = 220, MinHeight = 32 };
            comboTheme.Items.Add(LanguageManager.Get("Settings.Light"));
            comboTheme.Items.Add(LanguageManager.Get("Settings.Dark"));
            comboTheme.SelectedIndex     = _settings.AppTheme == "Light" ? 0 : 1;
            comboTheme.SelectionChanged += ComboTheme_Changed;
            AddSettingRow(generalStack, LanguageManager.Get("Settings.ThemeApp"), LanguageManager.Get("Settings.ThemeDescription"), comboTheme);

            var comboLanguage = new ComboBox { Width = 220, MinHeight = 32 };
            comboLanguage.Items.Add(LanguageManager.Get("Settings.French"));
            comboLanguage.Items.Add(LanguageManager.Get("Settings.English"));
            comboLanguage.SelectedIndex     = _settings.Language == "Français" ? 0 : 1;
            comboLanguage.SelectionChanged += ComboLanguage_Changed;
            AddSettingRow(generalStack, LanguageManager.Get("Settings.Language"), LanguageManager.Get("Settings.LanguageDescription"), comboLanguage, false);

            generalCard.Child = generalStack;
            content.Children.Add(generalCard);

            // ── SAVES ─────────────────────────────────────────────────────────
            AddSectionHeader(content, LanguageManager.Get("Settings.Saves"));

            var savesCard = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(20, 16, 20, 16), Margin = new Thickness(0, 0, 0, 20), BorderThickness = new Thickness(1) };
            savesCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            savesCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var savesStack = new StackPanel();
            var comboExecution = new ComboBox { Width = 220, MinHeight = 32 };
            comboExecution.Items.Add(LanguageManager.Get("Settings.Manual"));
            comboExecution.Items.Add(LanguageManager.Get("Settings.Auto"));
            comboExecution.SelectedIndex     = _settings.AutoExecute ? 1 : 0;
            comboExecution.SelectionChanged += ComboExecution_Changed;
            AddSettingRow(savesStack, LanguageManager.Get("Settings.ExecutionMode"), LanguageManager.Get("Settings.ExecutionDescription"), comboExecution, false);

            savesCard.Child = savesStack;
            content.Children.Add(savesCard);

            // ── LOGS ──────────────────────────────────────────────────────────
            AddSectionHeader(content, LanguageManager.Get("Settings.Logs"));

            var logsCard = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(20, 16, 20, 16), Margin = new Thickness(0, 0, 0, 20), BorderThickness = new Thickness(1) };
            logsCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            logsCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var logsStack    = new StackPanel();

            // Log file type (JSON / XML)
            var comboLogType = new ComboBox { Width = 220, MinHeight = 32 };
            comboLogType.Items.Add(LanguageManager.Get("Settings.JSON"));
            comboLogType.Items.Add(LanguageManager.Get("Settings.XML"));
            comboLogType.SelectedIndex     = _settings.LogFileType == "JSON" ? 0 : 1;
            comboLogType.SelectionChanged += ComboLogType_Changed;
            AddSettingRow(logsStack, LanguageManager.Get("Settings.FileType"), LanguageManager.Get("Settings.FileTypeDescription"), comboLogType);

            // ── Daily log path picker ─────────────────────────────────────────
            // Row layout: [path TextBlock (truncated)]  [Browse button]
            var pathPickerRow = new Grid();
            pathPickerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pathPickerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var currentPathBlock = new TextBlock
            {
                Text                = string.IsNullOrWhiteSpace(_settings.DailyLogPath)
                                          ? "Aucun dossier sélectionné"
                                          : _settings.DailyLogPath,
                FontSize            = 11,
                VerticalAlignment   = VerticalAlignment.Center,
                TextTrimming        = TextTrimming.CharacterEllipsis,
                FontFamily          = (FontFamily)FindResource("AppFont"),
                Margin              = new Thickness(0, 0, 10, 0),
            };
            currentPathBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            Grid.SetColumn(currentPathBlock, 0);
            pathPickerRow.Children.Add(currentPathBlock);

            var btnBrowse = new Button
            {
                Content   = "Parcourir…",
                MinHeight = 32,
                MinWidth  = 110,
            };
            btnBrowse.Click += (_, __) =>
            {
                // Même pattern que CreateWorkDialog : OpenFileDialog avec FileName = "Select Folder"
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title           = "Sélectionnez le dossier de destination pour le log journalier",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName        = "Select Folder",
                    InitialDirectory = string.IsNullOrWhiteSpace(_settings.DailyLogPath)
                                          ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                          : _settings.DailyLogPath,
                };

                if (dialog.ShowDialog() == true)
                {
                    var selectedPath        = System.IO.Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
                    _settings.DailyLogPath  = selectedPath;
                    currentPathBlock.Text   = selectedPath;
                    SettingsManager.Save(_settings);
                }
            };
            Grid.SetColumn(btnBrowse, 1);
            pathPickerRow.Children.Add(btnBrowse);

            // Wrap the picker inside a labelled setting row (reuse AddSettingRow with a custom control)
            AddSettingRow(
                logsStack,
                "Dossier du log journalier",
                "Chemin où sera écrit le fichier de log global (DailyLogPath).",
                pathPickerRow,
                false
            );

            logsCard.Child = logsStack;
            content.Children.Add(logsCard);

            scrollViewer.Content = content;
            SetContent(scrollViewer);

            // ── PRIORITY FILES ────────────────────────────────────────────────
            AddSectionHeader(content, "Fichiers prioritaires");

            var prioCard = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20, 16, 20, 16),
                Margin = new Thickness(0, 0, 0, 20),
                BorderThickness = new Thickness(1)
            };
            prioCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            prioCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var prioStack = new StackPanel();

            var prioTitle = new TextBlock
            {
                Text = "Formats prioritaires",
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            prioTitle.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            prioStack.Children.Add(prioTitle);

            var prioDesc = new TextBlock
            {
                Text = "Cochez les extensions à traiter en priorité, et ajoutez vos propres formats.",
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 10),
                FontFamily = (FontFamily)FindResource("AppFont")
            };
            prioDesc.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            prioStack.Children.Add(prioDesc);

            var box = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                BorderThickness = new Thickness(1),
            };
            box.SetResourceReference(Border.BorderBrushProperty, "BorderSubtleBrush");
            box.SetResourceReference(Border.BackgroundProperty, "CardBrush");

            var wrap = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };

            var defaultExtensions = new[]
            {
                ".json", ".xml", ".docx", ".xlsx", ".pptx", ".pdf",
                ".yml", ".yaml", ".svg", ".xaml", ".exe", ".secret"
            };

            _settings.CustomExtensions ??= new List<string>();

            var allExtensions = defaultExtensions
                .Concat(_settings.CustomExtensions)
                .Select(NormalizeExtension)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            foreach (var ext in allExtensions)
            {
                var cb = new CheckBox
                {
                    Content   = ext,
                    Margin    = new Thickness(0, 0, 24, 10),
                    IsChecked = _settings.PriorityExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)
                };
                cb.SetResourceReference(Control.ForegroundProperty, "ForegroundBrush");
                cb.Checked   += (_, __) => SetPriorityExtension(ext, true);
                cb.Unchecked += (_, __) => SetPriorityExtension(ext, false);
                wrap.Children.Add(cb);
            }

            var addRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };

            var tbAddExt = new TextBox { Width = 160, MinHeight = 30, Margin = new Thickness(0, 0, 10, 0) };

            var btnAddExt = new Button { Content = "Ajouter", MinHeight = 30 };
            btnAddExt.Click += (_, __) =>
            {
                var extToAdd = NormalizeExtension(tbAddExt.Text);
                if (string.IsNullOrWhiteSpace(extToAdd)) return;

                _settings.CustomExtensions ??= new List<string>();

                if (!_settings.CustomExtensions.Any(x => string.Equals(x, extToAdd, StringComparison.OrdinalIgnoreCase)))
                    _settings.CustomExtensions.Add(extToAdd);

                SetPriorityExtension(extToAdd, false);
                SettingsManager.Save(_settings);
                SectionSettings();
            };

            addRow.Children.Add(tbAddExt);
            addRow.Children.Add(btnAddExt);

            var inner = new StackPanel();
            inner.Children.Add(wrap);
            inner.Children.Add(addRow);
            box.Child = inner;

            prioStack.Children.Add(box);
            prioCard.Child = prioStack;

            content.Children.Add(prioCard);
        }

        private void AddSectionHeader(StackPanel parent, string text)
        {
            var header = new TextBlock
            {
                Text       = text,
                FontSize   = 15,
                FontWeight = FontWeights.SemiBold,
                Margin     = new Thickness(0, 0, 0, 10),
                FontFamily = (FontFamily)FindResource("AppFont"),
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

            var labelBlock = new TextBlock { Text = label, FontSize = 13, FontWeight = FontWeights.Medium, FontFamily = (FontFamily)FindResource("AppFont") };
            labelBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundBrush");
            textStack.Children.Add(labelBlock);

            var descBlock = new TextBlock { Text = description, FontSize = 11, Margin = new Thickness(0, 2, 0, 0), FontFamily = (FontFamily)FindResource("AppFont") };
            descBlock.SetResourceReference(TextBlock.ForegroundProperty, "ForegroundSecondaryBrush");
            textStack.Children.Add(descBlock);

            Grid.SetColumn(textStack, 0);
            row.Children.Add(textStack);

            var wrapper = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            wrapper.Children.Add(control);
            Grid.SetColumn(wrapper, 1);
            row.Children.Add(wrapper);

            parent.Children.Add(row);

            if (hasSeparator)
            {
                var sep = new Border { Height = 1 };
                sep.SetResourceReference(Border.BackgroundProperty, "BorderSubtleBrush");
                parent.Children.Add(sep);
            }
        }

        private void SetContent(UIElement content)
        {
            if (Template1.Visibility == Visibility.Visible)
                ContentTemplate1.Content = content;
            else
                ContentTemplate2.Content = content;
        }

        #endregion

        #region Event Handlers - Saves Section

        private void BtnCreateWork_Click()
        {
            var dialog = new CreateWorkDialog { Owner = this };

            if (dialog.ShowDialog() == true)
            {
                _vm.BackupCreateRequest = new BackupCreateRequest
                {
                    Name                = dialog.WorkName,
                    SourceFilePath      = dialog.SourcePath,
                    DestinationFilePath = dialog.DestinationPath,
                    Type                = dialog.SaveType == "Complete" ? BackupType.Full : BackupType.Sequential,
                };
                _vm.CreateBackupCommand.Execute(null);
            }
        }

        private void BtnExecuteWorks_Click(object sender, RoutedEventArgs e) { }

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
                if (Template1.Visibility == Visibility.Visible) ContentTemplate1.Content = currentSection;
                else ContentTemplate2.Content = currentSection;
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
                SectionSettings();
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

        private string NormalizeExtension(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var ext = input.Trim().ToLowerInvariant();

            if (!ext.StartsWith("."))
                ext = "." + ext;

            if (ext.Length < 2) return "";
            if (ext.Any(char.IsWhiteSpace)) return "";

            return ext;
        }

        private void SetPriorityExtension(string ext, bool enabled)
        {
            if (_isInitializing) return;

            ext = NormalizeExtension(ext);
            if (string.IsNullOrWhiteSpace(ext)) return;

            _settings.PriorityExtensions ??= new List<string>();

            var existing = _settings.PriorityExtensions
                .FirstOrDefault(x => string.Equals(x, ext, StringComparison.OrdinalIgnoreCase));

            if (enabled)
            {
                if (existing == null)
                    _settings.PriorityExtensions.Add(ext);
            }
            else
            {
                if (existing != null)
                    _settings.PriorityExtensions.Remove(existing);
            }

            SettingsManager.Save(_settings);
        }

        #endregion
    }
}
