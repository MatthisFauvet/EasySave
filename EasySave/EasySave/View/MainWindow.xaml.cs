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

namespace EasySave.View
{
    public partial class MainWindow : Window
    {
        private AppSettings _settings;
        private bool _isInitializing = true;
        private string _currentSection = "home";
        private MainViewModel _vm;

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
            BtnSettings1.Content = LanguageManager.Get("Navigation.Settings");

            BtnHome2.Content   = LanguageManager.Get("Navigation.Home");
            BtnSaves2.Content  = LanguageManager.Get("Navigation.Saves");
            BtnHistory2.Content = LanguageManager.Get("Navigation.History");
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
        private void NavHistory_Click(object sender, RoutedEventArgs e)  => SectionHistory();
        private void NavSettings_Click(object sender, RoutedEventArgs e) => SectionSettings();

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

            // Render existing items (BackupProgressItem wraps each Backup)
            foreach (var item in _vm.BackupItems)
                AddWorkItem(listPanel, item);

            // Sync list panel when BackupItems collection changes (page reload, add, remove)
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
                Content                  = "◀",
                Width                    = 48,
                Height                   = 32,
                Margin                   = new Thickness(0, 0, 8, 0),
                Foreground               = Brushes.White,
                Padding                  = new Thickness(0),
                FontSize                 = 18,
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
                Content                  = "▶",
                Width                    = 48,
                Height                   = 32,
                Margin                   = new Thickness(8, 0, 0, 0),
                Foreground               = Brushes.White,
                Padding                  = new Thickness(0),
                FontSize                 = 18,
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

        /// <summary>
        /// Builds the card UI for one backup.
        /// The card DataContext is set to the BackupProgressItem so all bindings are automatic.
        /// </summary>
        private void AddWorkItem(StackPanel parent, BackupProgressItem item)
        {
            var backup = item.Backup;

            var card = new Border
            {
                CornerRadius    = new CornerRadius(8),
                Padding         = new Thickness(16, 12, 16, 12),
                Margin          = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(1),
                Tag             = item,           // used by RemoveWorkItem
                DataContext     = item            // all bindings inside resolve against BackupProgressItem
            };
            card.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            card.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var stack = new StackPanel();

            // ── TOP ROW : name / badge / pause / delete ───────────────────────
            var topRow = new Grid();
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // type badge
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // pause button
            topRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // delete button

            // Name
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

            // Type badge
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

            // ── PAUSE / PLAY BUTTON ────────────────────────────────────────────
            var pauseButton = BuildIconButton("⏸", margin: new Thickness(8, 0, 0, 0));

            // The emoji toggles between ⏸ and ▶ based on IsPaused
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

            // ── DELETE BUTTON ─────────────────────────────────────────────────
            var deleteButton = BuildIconButton("🗑", margin: new Thickness(8, 0, 0, 0));
            deleteButton.Click += (_, _) =>
            {
                _vm.RemoveBackup(backup);
                parent.Children.Remove(card);
            };
            Grid.SetColumn(deleteButton, 3);
            topRow.Children.Add(deleteButton);

            stack.Children.Add(topRow);

            // ── PROGRESS ROW ─────────────────────────────────────────────────
            var progressRow = new Grid { Margin = new Thickness(0, 10, 0, 0) };
            progressRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            progressRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // FIX: set DataContext explicitly — do NOT rely on card.DataContext propagation
            // because bindings are evaluated at SetBinding() time, before the visual tree is built.
            // Use BuildColorableProgressBar() so Foreground color changes are actually visible —
            // the default WPF ProgressBar template ignores Foreground entirely.
            var progressBar = BuildColorableProgressBar();
            progressBar.DataContext = item;
            progressBar.SetBinding(ProgressBar.ValueProperty, new Binding("Progress"));
            Grid.SetColumn(progressBar, 0);
            progressRow.Children.Add(progressBar);

            // "12 / 48 files  (25%)"
            var progressLabel = new TextBlock
            {
                DataContext       = item,   // explicit for the same reason
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

            // ── COLOR: change progress bar to green when completed ────────────
            // WPF ProgressBar ignores Foreground changes after render unless we force
            // a style override. We listen to PropertyChanged and swap the Foreground brush.
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(BackupProgressItem.Status) &&
                    e.PropertyName != nameof(BackupProgressItem.Progress)) return;

                progressBar.Foreground = item.Status switch
                {
                    BackupStatus.Completed => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34D399")), // green
                    BackupStatus.Error     => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F87171")), // red
                    BackupStatus.Cancelled => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24")), // yellow
                    BackupStatus.Paused    => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A78BFA")), // purple
                    _                      => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60A5FA")), // blue (running/idle)
                };
            };

            card.Child = stack;
            parent.Children.Add(card);
        }

        /// <summary>
        /// Creates a ProgressBar with a custom template that actually respects Foreground.
        /// The default WPF ProgressBar template ignores Foreground and uses an internal accent brush,
        /// so color changes via Foreground have no visible effect without this override.
        /// </summary>
        private static ProgressBar BuildColorableProgressBar()
        {
            // The indicator rectangle fills according to the ProgressBar value
            // We bind its Fill to the ProgressBar's Foreground so we can change the color at runtime
            var indicatorFill = new FrameworkElementFactory(typeof(Rectangle));
            indicatorFill.SetValue(Rectangle.RadiusXProperty, 3.0);
            indicatorFill.SetValue(Rectangle.RadiusYProperty, 3.0);
            indicatorFill.SetBinding(
                Rectangle.FillProperty,
                new Binding("Foreground") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );

            // The indicator stretches left-to-right based on the value
            var indicatorGrid = new FrameworkElementFactory(typeof(Grid));
            indicatorGrid.SetValue(Grid.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            indicatorGrid.AppendChild(indicatorFill);

            // A trigger animates Width via TemplatedParent binding — we do it manually instead:
            // Wrap inside a Grid that clips to the correct width via a ScaleTransform on a border
            var track = new FrameworkElementFactory(typeof(Border));
            track.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            track.SetValue(Border.ClipToBoundsProperty, true);
            track.SetBinding(
                Border.BackgroundProperty,
                new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) }
            );

            // Inner bar — sized by WPF's built-in PART_Track / PART_Indicator mechanism
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
                Background        = new SolidColorBrush(Color.FromArgb(40, 96, 165, 250)), // faint blue track
                Foreground        = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60A5FA")),
                Height            = 6,
                Minimum           = 0,
                Maximum           = 100,
                Value             = 0,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        /// <summary>
        /// Builds a minimal transparent icon button (emoji label, no border).
        /// </summary>
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

            AddSectionHeader(content, LanguageManager.Get("Settings.General"));

            var generalCard = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(20, 16, 20, 16), Margin = new Thickness(0, 0, 0, 20), BorderThickness = new Thickness(1) };
            generalCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            generalCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var generalStack = new StackPanel();

            var comboTemplate = new ComboBox { Width = 220, MinHeight = 32 };
            comboTemplate.Items.Add(LanguageManager.Get("Settings.Template1"));
            comboTemplate.Items.Add(LanguageManager.Get("Settings.Template2"));
            comboTemplate.SelectedIndex        = _settings.AppTemplate - 1;
            comboTemplate.SelectionChanged    += ComboTemplate_Changed;
            AddSettingRow(generalStack, LanguageManager.Get("Settings.TemplateApp"), LanguageManager.Get("Settings.TemplateDescription"), comboTemplate);

            var comboTheme = new ComboBox { Width = 220, MinHeight = 32 };
            comboTheme.Items.Add(LanguageManager.Get("Settings.Light"));
            comboTheme.Items.Add(LanguageManager.Get("Settings.Dark"));
            comboTheme.SelectedIndex        = _settings.AppTheme == "Light" ? 0 : 1;
            comboTheme.SelectionChanged    += ComboTheme_Changed;
            AddSettingRow(generalStack, LanguageManager.Get("Settings.ThemeApp"), LanguageManager.Get("Settings.ThemeDescription"), comboTheme);

            var comboLanguage = new ComboBox { Width = 220, MinHeight = 32 };
            comboLanguage.Items.Add(LanguageManager.Get("Settings.French"));
            comboLanguage.Items.Add(LanguageManager.Get("Settings.English"));
            comboLanguage.SelectedIndex      = _settings.Language == "Français" ? 0 : 1;
            comboLanguage.SelectionChanged  += ComboLanguage_Changed;
            AddSettingRow(generalStack, LanguageManager.Get("Settings.Language"), LanguageManager.Get("Settings.LanguageDescription"), comboLanguage, false);

            generalCard.Child = generalStack;
            content.Children.Add(generalCard);

            AddSectionHeader(content, LanguageManager.Get("Settings.Saves"));

            var savesCard = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(20, 16, 20, 16), Margin = new Thickness(0, 0, 0, 20), BorderThickness = new Thickness(1) };
            savesCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            savesCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var savesStack = new StackPanel();
            var comboExecution = new ComboBox { Width = 220, MinHeight = 32 };
            comboExecution.Items.Add(LanguageManager.Get("Settings.Manual"));
            comboExecution.Items.Add(LanguageManager.Get("Settings.Auto"));
            comboExecution.SelectedIndex      = _settings.AutoExecute ? 1 : 0;
            comboExecution.SelectionChanged  += ComboExecution_Changed;
            AddSettingRow(savesStack, LanguageManager.Get("Settings.ExecutionMode"), LanguageManager.Get("Settings.ExecutionDescription"), comboExecution, false);

            savesCard.Child = savesStack;
            content.Children.Add(savesCard);

            AddSectionHeader(content, LanguageManager.Get("Settings.Logs"));

            var logsCard = new Border { CornerRadius = new CornerRadius(8), Padding = new Thickness(20, 16, 20, 16), Margin = new Thickness(0, 0, 0, 20), BorderThickness = new Thickness(1) };
            logsCard.SetResourceReference(Border.BackgroundProperty, "CardBrush");
            logsCard.SetResourceReference(Border.BorderBrushProperty, "BorderBrush");

            var logsStack    = new StackPanel();
            var comboLogType = new ComboBox { Width = 220, MinHeight = 32 };
            comboLogType.Items.Add(LanguageManager.Get("Settings.JSON"));
            comboLogType.Items.Add(LanguageManager.Get("Settings.XML"));
            comboLogType.SelectedIndex      = _settings.LogFileType == "JSON" ? 0 : 1;
            comboLogType.SelectionChanged  += ComboLogType_Changed;
            AddSettingRow(logsStack, LanguageManager.Get("Settings.FileType"), LanguageManager.Get("Settings.FileTypeDescription"), comboLogType, false);

            logsCard.Child = logsStack;
            content.Children.Add(logsCard);

            scrollViewer.Content = content;
            SetContent(scrollViewer);
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

        #endregion
    }
}
