using QuickViewFile;
using QuickViewFile.Controls;
using QuickViewFile.Helpers;
using QuickViewFile.Models;
using QuickViewFile.ViewModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuickViewFile
{
    public partial class MainWindow : Window
    {
        private bool _filesListViewVisible = true;
        private readonly ConfigModel _config;
        private GridLength _filesListColumnWidthCache;
        private int degreesRotation = 0;
        public FilesListViewModel vm;

        public MainWindow()
        {
            try
            {
                RenderOptions.SetCachingHint(this, CachingHint.Cache);
                this.UseLayoutRounding = true;
                _config = ConfigHelper.loadedConfig;
                RenderOptions.ProcessRenderMode = _config.RenderMode == 0 ? System.Windows.Interop.RenderMode.Default : System.Windows.Interop.RenderMode.SoftwareOnly;
                RenderOptions.SetEdgeMode(this, _config.EdgeMode == 1 ? EdgeMode.Aliased : EdgeMode.Unspecified);
                if (_config.ShadowEffect == 1)
                {
                    System.Windows.Media.Effects.DropShadowEffect dropShadow = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        ShadowDepth = _config.ShadowDepth,
                        Opacity = _config.ShadowOpacity,
                        BlurRadius = _config.ShadowBlur,
                        RenderingBias = _config.ShadowQuality == 1 ? System.Windows.Media.Effects.RenderingBias.Quality : System.Windows.Media.Effects.RenderingBias.Performance,
                    };
                    Effect = dropShadow;
                }
                if (_config.ThemeMode == 2)
                {
#pragma warning disable WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    Application.Current.ThemeMode = ThemeMode.Dark;
#pragma warning restore WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                }
                else if (_config.ThemeMode == 1)
                {
#pragma warning disable WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    Application.Current.ThemeMode = ThemeMode.Light;
#pragma warning restore WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                }
                InitializeComponent();
                FilesListView.Focus();

                string[] args = Environment.GetCommandLineArgs();

                if (!String.IsNullOrWhiteSpace(args.ElementAtOrDefault(1)))
                {
                    string fileToSelectFullPath = args.ElementAt(1);
                    if (File.Exists(fileToSelectFullPath))
                    {
                        vm = new FilesListViewModel(fileToSelectFullPath);
                        DataContext = vm;
                    }
                }
                else
                {
                    FilesListViewModel vm = new FilesListViewModel(Directory.GetCurrentDirectory());
                    DataContext = vm;

                }
            }
            catch
            {
                FilesListViewModel vm = new FilesListViewModel(Directory.GetCurrentDirectory());
                DataContext = vm;
            }
        }

        public MainWindow(string pathNoBorder)
        {
            try
            {
                RenderOptions.SetCachingHint(this, CachingHint.Cache);
                this.UseLayoutRounding = true;
                _config = ConfigHelper.loadedConfig;
                RenderOptions.ProcessRenderMode = _config.RenderMode == 0 ? System.Windows.Interop.RenderMode.Default : System.Windows.Interop.RenderMode.SoftwareOnly;
                RenderOptions.SetEdgeMode(this, _config.EdgeMode == 1 ? EdgeMode.Aliased : EdgeMode.Unspecified);
                if (_config.ShadowEffect == 1)
                {
                    System.Windows.Media.Effects.DropShadowEffect dropShadow = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        ShadowDepth = _config.ShadowDepth,
                        Opacity = _config.ShadowOpacity,
                        BlurRadius = _config.ShadowBlur,
                        RenderingBias = _config.ShadowQuality == 1 ? System.Windows.Media.Effects.RenderingBias.Quality : System.Windows.Media.Effects.RenderingBias.Performance,
                    };
                    Effect = dropShadow;
                }
                if (_config.ThemeMode == 2)
                {
#pragma warning disable WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    Application.Current.ThemeMode = ThemeMode.Dark;
#pragma warning restore WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                }
                else if (_config.ThemeMode == 1)
                {
#pragma warning disable WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    Application.Current.ThemeMode = ThemeMode.Light;
#pragma warning restore WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                }
                InitializeComponent();
                FilesListView.Focus();

                string fileToSelectFullPath = pathNoBorder;

                vm = new FilesListViewModel(fileToSelectFullPath);
                DataContext = vm;
            }
            catch
            {
                FilesListViewModel vm = new FilesListViewModel(Directory.GetCurrentDirectory());
                DataContext = vm;
            }
        }
        private void FilesListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) //Change directory or force load file (using double click)
        {
            if (DataContext is FilesListViewModel vm)
            {
                if (sender is ListView listView && listView.SelectedItem is QuickViewFile.Models.ItemList file)
                {
                    Application.Current.Dispatcher.BeginInvoke(async () =>
                    {
                        await vm.OnFileDoubleClick(file);
                    });
                }
            }
        }


        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && vm.SelectedItem?.FullPath is not null)
            {
                MainWindowNoBorder fullScreen = new MainWindowNoBorder(vm.SelectedItem.FullPath);
                if (vm.SelectedItem.FileContentModel.VideoMedia is not null)
                {
                    vm.SelectedItem.FileContentModel.VideoMedia.StopForce();
                    vm.SelectedItem.FileContentModel.VideoMedia.Dispose();
                }
                fullScreen.Show();
                this.Close();
            }
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            degreesRotation += 90;
            if (degreesRotation < 360)
            {
                GridFileContent.LayoutTransform = new System.Windows.Media.RotateTransform(degreesRotation);
            }
            else
            {
                degreesRotation = 0;
                GridFileContent.LayoutTransform = new System.Windows.Media.RotateTransform(0);
            }
        }

        private void HideUIButton_Click(object sender, RoutedEventArgs e)
        {
            HideUI();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            if (settingsWindow.ShowDialog() == true)
            {
                if (DataContext is FilesListViewModel vm)
                {
                    vm.Config = ConfigHelper.LoadConfig();
                }
            }
        }
        private SlideshowHelper? _slideshowHelper;
        private void SlideshowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_slideshowHelper != null)
            {
                _slideshowHelper.Stop();
                _slideshowHelper = null;
                SlideshowButton.Content = "Slideshow";
                return;
            }

            SlideshowSettingsWindow settingsWindow = new SlideshowSettingsWindow();
            settingsWindow.Owner = this;
            if (settingsWindow.ShowDialog() == true)
            {
                // start slideshow
                _slideshowHelper = new SlideshowHelper(this, settingsWindow.SelectedMode, settingsWindow.SlideDuration, settingsWindow.AnimDuration, settingsWindow.FadeOpacity, settingsWindow.IsFastQuality);
                _slideshowHelper.Start();
                SlideshowButton.Content = "Stop Slideshow";
            }
        }
        private enum FileOperation { None, Copy, Move }
        private FileOperation _currentOperation = FileOperation.None;
        private List<string> _clipboardFiles = new List<string>();

        private void ThumbnailsModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm)
            {
                MainWindowThumbnails thumbnailsWindow = new MainWindowThumbnails(vm);
                thumbnailsWindow.Show();
                this.Close();
            }
        }

        private void MoveFiles_Click(object sender, RoutedEventArgs e)
        {
            _currentOperation = FileOperation.Move;
            UpdateClipboardFiles();
        }

        private void CopyFiles_Click(object sender, RoutedEventArgs e)
        {
            _currentOperation = FileOperation.Copy;
            UpdateClipboardFiles();
        }

        private IEnumerable<ItemList> GetSelectedOrCheckedItems()
        {
            var checkedItems = FilesListView.Items.Cast<ItemList>().Where(i => i.IsChecked).ToList();
            if (checkedItems.Count > 0) return checkedItems;

            var selectedItems = FilesListView.SelectedItems.Cast<ItemList>().ToList();
            return selectedItems;
        }

        // Zamień metody UpdateClipboardFiles i PasteFiles_Click na:

        private void UpdateClipboardFiles()
        {
            var itemsToProcess = GetSelectedOrCheckedItems();
            if (itemsToProcess.Any())
            {
                _clipboardFiles.Clear();
                foreach (ItemList item in itemsToProcess)
                {
                    // Dodaj zarówno foldery jak i pliki, ale wyklucz ".." i ADS
                    if (item.Name != ".." && !item.IsAlternativeDataStream)
                    {
                        _clipboardFiles.Add(item.FullPath);
                    }
                }
                if (_clipboardFiles.Count > 0)
                {
                    MoveButton.Visibility = Visibility.Collapsed;
                    CopyButton.Visibility = Visibility.Collapsed;
                    DeleteButton.Visibility = Visibility.Collapsed;
                    NewFolderButton.Visibility = Visibility.Collapsed;
                    PasteButton.Visibility = Visibility.Visible;
                    CancelPasteButton.Visibility = Visibility.Visible;

                    // Policz pliki i foldery (bezpieczna iteracja bez wyjątków dla nieistniejących ścieżek)
                    int fileCount = 0;
                    int folderCount = 0;

                    foreach (var path in _clipboardFiles)
                    {
                        try
                        {
                            if (Directory.Exists(path))
                                folderCount++;
                            else if (File.Exists(path))
                                fileCount++;
                        }
                        catch
                        {
                            // Bezpiecznie ignoruj błędy dostępu
                            fileCount++;
                        }
                    }

                    string itemsLabel = "";
                    if (fileCount > 0 && folderCount > 0)
                        itemsLabel = $"Paste ({folderCount} folders, {fileCount} files)";
                    else if (folderCount > 0)
                        itemsLabel = $"Paste ({folderCount} folders)";
                    else
                        itemsLabel = $"Paste ({fileCount} files)";

                    PasteButton.Content = itemsLabel;
                }
            }
        }

        private void DeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm)
            {
                var itemsToDelete = GetSelectedOrCheckedItems().ToList();
                if (itemsToDelete.Count > 0)
                {
                    // Filtruj - wyklucz ADS i ".."
                    var validItems = itemsToDelete.Where(x => x.Name != ".." && !x.IsAlternativeDataStream).ToList();

                    if (validItems.Count == 0)
                        return;

                    var result = MessageBox.Show(
                        $"Are you sure you want to delete {validItems.Count} items?",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (ItemList item in validItems)
                        {
                            try
                            {
                                if (item.IsDirectory)
                                {
                                    DirectoryOperationHelper.DeleteDirectoryRecursive(item.FullPath);
                                }
                                else
                                {
                                    File.Delete(item.FullPath);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Failed to delete {item.Name}: {ex.Message}");
                            }
                        }
                        vm.RefreshFiles();
                    }
                }
            }
        }

        private async void PasteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_clipboardFiles.Count > 0 && DataContext is QuickViewFile.ViewModel.FilesListViewModel vm)
            {
                var clipboardCopy = new System.Collections.Generic.List<string>(_clipboardFiles);
                string targetDir = vm.FolderPath;
                int currentOp = _currentOperation == FileOperation.Copy ? 1 : (_currentOperation == FileOperation.Move ? 2 : 0);

                FileOperationsPanel.Visibility = Visibility.Collapsed;
                ProgressPanel.Visibility = Visibility.Visible;
                FilesListView.IsEnabled = false;
                OperationProgressBar.Value = 0;
                OperationStatusText.Text = "Preparing...";

                _pasteCts = new System.Threading.CancellationTokenSource();

                await PasteLogic.PerformPasteAsync(clipboardCopy, targetDir, currentOp, this, OperationProgressBar, OperationStatusText, _pasteCts, () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        FileOperationsPanel.Visibility = Visibility.Visible;
                        ProgressPanel.Visibility = Visibility.Collapsed;
                        FilesListView.IsEnabled = true;

                        _clipboardFiles.Clear();
                        _currentOperation = FileOperation.None;
                        PasteButton.Visibility = Visibility.Collapsed;
                        CancelPasteButton.Visibility = Visibility.Collapsed;
                        MoveButton.Visibility = Visibility.Visible;
                        CopyButton.Visibility = Visibility.Visible;
                        DeleteButton.Visibility = Visibility.Visible;
                        NewFolderButton.Visibility = Visibility.Visible;
                        vm.RefreshFiles();

                        _pasteCts?.Dispose();
                        _pasteCts = null;
                    });
                });
            }
        }

        //  Sorting
        private GridViewColumnHeader _lastHeaderClicked = null;
        private System.ComponentModel.ListSortDirection _lastDirection = System.ComponentModel.ListSortDirection.Ascending;

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = sender as GridViewColumnHeader;
            if (headerClicked != null && headerClicked.Tag != null)
            {
                string propertyName = headerClicked.Tag.ToString();

                System.ComponentModel.ListSortDirection direction;
                if (headerClicked != _lastHeaderClicked)
                {
                    direction = System.ComponentModel.ListSortDirection.Ascending;
                }
                else
                {
                    if (_lastDirection == System.ComponentModel.ListSortDirection.Ascending)
                    {
                        direction = System.ComponentModel.ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = System.ComponentModel.ListSortDirection.Ascending;
                    }
                }

                _lastHeaderClicked = headerClicked;
                _lastDirection = direction;

                SortFilesList(propertyName, direction);
            }
        }

        private void SortFilesList(string sortBy, System.ComponentModel.ListSortDirection direction)
        {
            if (DataContext is FilesListViewModel vm)
            {
                var folders = vm.ActiveListItems.Where(x => x.IsDirectory && x.Name != "..").ToList();
                var files = vm.ActiveListItems.Where(x => !x.IsDirectory).ToList();
                var parentDir = vm.ActiveListItems.Where(x => x.Name == "..").FirstOrDefault();

                if (sortBy == "Name")
                {
                    if (direction == System.ComponentModel.ListSortDirection.Ascending)
                    {
                        folders = folders.OrderBy(x => x.Name).ToList();
                        files = files.OrderBy(x => x.Name).ToList();
                    }
                    else
                    {
                        folders = folders.OrderByDescending(x => x.Name).ToList();
                        files = files.OrderByDescending(x => x.Name).ToList();
                    }
                }
                else if (sortBy == "Size")
                {
                    if (direction == System.ComponentModel.ListSortDirection.Ascending)
                        files = files.OrderBy(x => x.SizeBytes).ToList();
                    else
                        files = files.OrderByDescending(x => x.SizeBytes).ToList();
                }
                else if (sortBy == "LastModified")
                {
                    if (direction == System.ComponentModel.ListSortDirection.Ascending)
                    {
                        folders = folders.OrderBy(x => x.LastModified).ToList();
                        files = files.OrderBy(x => x.LastModified).ToList();
                    }
                    else
                    {
                        folders = folders.OrderByDescending(x => x.LastModified).ToList();
                        files = files.OrderByDescending(x => x.LastModified).ToList();
                    }
                }

                var newList = new System.Collections.ObjectModel.ObservableCollection<ItemList>();
                if (parentDir != null) newList.Add(parentDir);
                foreach (var folder in folders) newList.Add(folder);
                foreach (var file in files) newList.Add(file);
                vm.ActiveListItems = newList;
                FilesListView.ItemsSource = vm.ActiveListItems;
            }
        }


        private void AppWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox)
            {
                return;
            }

            if (System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox)
            {
                return;
            }

            if (e.Key == System.Windows.Input.Key.F5)
            {
                if (DataContext is QuickViewFile.ViewModel.FilesListViewModel vm1)
                    vm1.RefreshFiles();
                e.Handled = true;
                return;
            }

            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (DataContext is QuickViewFile.ViewModel.FilesListViewModel vm2)
                {
                    if (e.Key == System.Windows.Input.Key.A)
                    {
                        foreach (var item in vm2.ActiveListItems)
                        {
                            item.IsChecked = true;
                        }
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == System.Windows.Input.Key.C)
                    {
                        CopyFiles_Click(null, null);
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == System.Windows.Input.Key.X)
                    {
                        MoveFiles_Click(null, null);
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == System.Windows.Input.Key.V)
                    {
                        PasteFiles_Click(null, null);
                        e.Handled = true;
                        return;
                    }
                }
            }

            try
            {
                if (e.Key == System.Windows.Input.Key.F11)
                {
                    if (_filesListViewVisible) HideUI();
                    else ShowUI();
                    e.Handled = true;
                    return;
                }

                if (DataContext is QuickViewFile.ViewModel.FilesListViewModel vm)
                {
                    if (e.Key == System.Windows.Input.Key.F4 && vm.SelectedItem?.FullPath is not null)
                    {
                        MainWindowNoBorder fullScreen = new MainWindowNoBorder(vm.SelectedItem.FullPath);
                        if (vm.SelectedItem.FileContentModel.VideoMedia is not null)
                        {
                            vm.SelectedItem.FileContentModel.VideoMedia.StopForce();
                            vm.SelectedItem.FileContentModel.VideoMedia.Dispose();
                        }
                        fullScreen.Show();
                        this.Close();
                        e.Handled = true;
                        return;
                    }

                    bool isTextBoxFocused = System.Windows.Input.Keyboard.FocusedElement is TextBox;
                    bool isTextFileOpen = vm.SelectedItem?.FileContentModel?.ShowTextBox == true;

                    // If a text file is open, and focus is OUTSIDE the list (i.e. we are typing in text box),
                    // Do NOT intercept navigation keys!
                    if (isTextFileOpen && isTextBoxFocused)
                    {
                        if (e.Key == Key.Add)
                        {
                            TextBoxTextContent.FontSize += 0.5;
                            ConfigHelper.SetFontSize(TextBoxTextContent.FontSize);
                            e.Handled = true;
                        }
                        else if (e.Key == Key.Subtract)
                        {
                            if (TextBoxTextContent.FontSize >= 1)
                            {
                                TextBoxTextContent.FontSize -= 0.5;
                                ConfigHelper.SetFontSize(TextBoxTextContent.FontSize);
                            }
                            e.Handled = true;
                        }

                        // We return here so we don't accidentally navigate files while typing!
                        return;
                    }

                    if (e.Key == Key.Escape)
                    {
                        SearchTextBox.Text = string.Empty;
                        SearchResultsCount.Text = string.Empty;
                        _searchResults.Clear();
                        _currentSearchIndex = -1;
                        e.Handled = true;
                        return;
                    }

                    // Proceed with Global File Navigation
                    if (e.Key == Key.Multiply)
                    {
                        degreesRotation += 90;
                        if (degreesRotation < 360) GridFileContent.LayoutTransform = new RotateTransform(degreesRotation);
                        else
                        {
                            degreesRotation = 0;
                            GridFileContent.LayoutTransform = new RotateTransform(0);
                        }
                        e.Handled = true;
                        return;
                    }

                    if (e.Key == Key.Space)
                    {
                        var focusedElement = System.Windows.Input.Keyboard.FocusedElement as ListViewItem;
                        if (focusedElement == null)
                        {
                            // Fallback to selected item if focus is lost but something is selected
                            if (FilesListView.SelectedItem != null)
                            {
                                focusedElement = FilesListView.ItemContainerGenerator.ContainerFromItem(FilesListView.SelectedItem) as ListViewItem;
                            }
                        }

                        if (focusedElement != null)
                        {
                            var itemData = FilesListView.ItemContainerGenerator.ItemFromContainer(focusedElement) as QuickViewFile.Models.ItemList;

                            //if(itemData)

                            if (itemData != null)
                            {
                                itemData.IsChecked = !itemData.IsChecked;

                                int currentIndex = FilesListView.ItemContainerGenerator.IndexFromContainer(focusedElement);
                            }
                        }
                        e.Handled = true;
                        return;
                    }

                    if (e.Key >= Key.A && e.Key <= Key.Z)
                    {
                        char ASCIINumberWhichUserWantToSelect = e.Key.ToString()[0];
                        var itemToSelect = FilesListView.Items.Cast<QuickViewFile.Models.ItemList>().Skip(FilesListView.SelectedIndex + 1)
                            .FirstOrDefault(item => !string.IsNullOrEmpty(item.Name) && char.ToUpper(item.Name[0]) == ASCIINumberWhichUserWantToSelect);

                        if (itemToSelect is null)
                        {
                            itemToSelect = FilesListView.Items.Cast<QuickViewFile.Models.ItemList>()
                                .FirstOrDefault(item => !string.IsNullOrEmpty(item.Name) && char.ToUpper(item.Name[0]) == ASCIINumberWhichUserWantToSelect);
                        }
                        if (itemToSelect is not null)
                        {
                            FilesListView.SelectedItem = itemToSelect;
                            FilesListView.ScrollIntoView(itemToSelect);
                            if (DataContext is FilesListViewModel viewm && FilesListView.SelectedItem is ItemList sel) viewm.SelectedItem = sel;
                        }
                        e.Handled = true;
                        return;
                    }

                    try
                    {
                        bool handled = false;
                        if (e.Key == Key.Right || e.Key == Key.Down)
                        {
                            if (FilesListView.SelectedIndex < FilesListView.Items.Count - 1)
                                FilesListView.SelectedIndex++;
                            handled = true;
                        }
                        else if (e.Key == Key.Left || e.Key == Key.Up)
                        {
                            if (FilesListView.SelectedIndex > 0)
                                FilesListView.SelectedIndex--;
                            handled = true;
                        }
                        else if (e.Key == Key.PageDown)
                        {
                            FilesListView.SelectedIndex = Math.Min(FilesListView.Items.Count - 1, FilesListView.SelectedIndex + 10);
                            handled = true;
                        }
                        else if (e.Key == Key.PageUp)
                        {
                            FilesListView.SelectedIndex = Math.Max(0, FilesListView.SelectedIndex - 10);
                            handled = true;
                        }
                        else if (e.Key == Key.Home)
                        {
                            FilesListView.SelectedIndex = 0;
                            handled = true;
                        }
                        else if (e.Key == Key.End)
                        {
                            FilesListView.SelectedIndex = FilesListView.Items.Count - 1;
                            handled = true;
                        }

                        if (handled)
                        {
                            FilesListView.SetCurrentValue(ListView.SelectedIndexProperty, FilesListView.SelectedIndex);
                            FilesListView.ScrollIntoView(FilesListView.SelectedItem);
                            e.Handled = true;
                            return;
                        }
                    }
                    catch { }

                    if (e.Key == Key.Enter)
                    {
                        Application.Current.Dispatcher.BeginInvoke(async () =>
                        {
                            await vm.OnFileDoubleClick(vm.SelectedItem);
                        });
                        e.Handled = true;
                    }
                }
            }
            catch { }
        }



        private int _previousSelectedIndex = -1;

        private void TriggerSwipeAnimation(bool isNext)
        {
            double startX = isNext ? GridFileContent.ActualWidth : -GridFileContent.ActualWidth;
            System.Windows.Media.Animation.DoubleAnimation translateAnim = new System.Windows.Media.Animation.DoubleAnimation(startX, 0, TimeSpan.FromSeconds(0.3));
            translateAnim.EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };

            FileContentTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, translateAnim);
        }

        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && FilesListView.SelectedItem is ItemList selected)
            {
                vm.SelectedItem = selected;

                int newIndex = FilesListView.SelectedIndex;
                if (_config.AlwaysSwipeAnimation == 1 && _previousSelectedIndex != -1 && newIndex != _previousSelectedIndex)
                {
                    bool isNext = newIndex > _previousSelectedIndex;

                    // Handle edge cases (looping from end to start or start to end)
                    if (_previousSelectedIndex == FilesListView.Items.Count - 1 && newIndex == 0) isNext = true;
                    if (_previousSelectedIndex == 0 && newIndex == FilesListView.Items.Count - 1) isNext = false;

                    TriggerSwipeAnimation(isNext);
                }
                _previousSelectedIndex = newIndex;
            }
        }

        private void FilesListView_KeyDown(object sender, KeyEventArgs e)
        {
            // Navigation handled globally
        }



        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }

        private Point? _mouseDownPosition;
        private DateTime _mouseDownTime;

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject depObj)
            {
                if (FindVisualParent<GridViewColumnHeader>(depObj) != null ||
                    FindVisualParent<ListViewItem>(depObj) != null ||
                    FindVisualParent<System.Windows.Controls.Primitives.ScrollBar>(depObj) != null ||
                    FindVisualParent<Button>(depObj) != null ||
                    FindVisualParent<TextBox>(depObj) != null)
                {
                    return;
                }
            }

            _mouseDownPosition = e.GetPosition(this);
            _mouseDownTime = DateTime.Now;
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_mouseDownPosition == null) return;

            Point mouseUpPosition = e.GetPosition(this);
            double distance = Math.Sqrt(Math.Pow(mouseUpPosition.X - _mouseDownPosition.Value.X, 2) + Math.Pow(mouseUpPosition.Y - _mouseDownPosition.Value.Y, 2));
            TimeSpan elapsed = DateTime.Now - _mouseDownTime;

            _mouseDownPosition = null;

            if (distance < 10 && elapsed.TotalMilliseconds < 500)
            {
                try
                {
                    if (DataContext is FilesListViewModel vm)
                    {
                        bool goPrevious = false;
                        bool goNext = false;

                        Point pApp = e.GetPosition(GridFileContent);
                        double wApp = GridFileContent.ActualWidth;

                        if (wApp > 0 && pApp.X >= 0 && pApp.X <= wApp && pApp.Y >= 0 && pApp.Y <= GridFileContent.ActualHeight)
                        {
                            if (pApp.X < wApp * 0.15) goPrevious = true;
                            else if (pApp.X > wApp * 0.85) goNext = true;
                        }

                        if (!goPrevious && !goNext && ZoomableImageElement.Visibility == Visibility.Visible && ZoomableImageElement.Source != null)
                        {
                            var image = ZoomableImageElement;
                            double imageWidth = image.Source.Width;
                            double imageHeight = image.Source.Height;
                            double controlWidth = image.ActualWidth;
                            double controlHeight = image.ActualHeight;

                            double scaleX = controlWidth / imageWidth;
                            double scaleY = controlHeight / imageHeight;
                            double scale = Math.Min(scaleX, scaleY);

                            double drawnWidth = imageWidth * scale;
                            double offsetX = (controlWidth - drawnWidth) / 2;

                            Point pImg = e.GetPosition(image);

                            if (pImg.X >= offsetX && pImg.X <= offsetX + drawnWidth)
                            {
                                double relativeX = pImg.X - offsetX;
                                if (relativeX < drawnWidth * 0.15) goPrevious = true;
                                else if (relativeX > drawnWidth * 0.85) goNext = true;
                            }
                        }

                        if (!goPrevious && !goNext && VideoMedia.Visibility == Visibility.Visible && VideoMedia.videoInWindowPlayer.NaturalVideoWidth > 0)
                        {
                            double videoWidth = VideoMedia.videoInWindowPlayer.NaturalVideoWidth;
                            double videoHeight = VideoMedia.videoInWindowPlayer.NaturalVideoHeight;
                            double controlWidth = VideoMedia.videoInWindowPlayer.ActualWidth;
                            double controlHeight = VideoMedia.videoInWindowPlayer.ActualHeight;

                            double scaleX = controlWidth / videoWidth;
                            double scaleY = controlHeight / videoHeight;
                            double scale = Math.Min(scaleX, scaleY);

                            double drawnWidth = videoWidth * scale;
                            double offsetX = (controlWidth - drawnWidth) / 2;

                            Point pVid = e.GetPosition(VideoMedia.videoInWindowPlayer);

                            if (pVid.X >= offsetX && pVid.X <= offsetX + drawnWidth)
                            {
                                double relativeX = pVid.X - offsetX;
                                if (relativeX < drawnWidth * 0.15) goPrevious = true;
                                else if (relativeX > drawnWidth * 0.85) goNext = true;
                            }
                        }

                        if (goPrevious && FilesListView.SelectedIndex > 0)
                        {
                            FilesListView.SelectedIndex--;
                            System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.None);
                            System.Windows.Input.Mouse.OverrideCursor = null;
                            System.Windows.Input.Mouse.UpdateCursor();
                        }
                        else if (goNext && FilesListView.SelectedIndex < FilesListView.Items.Count - 1)
                        {
                            FilesListView.SelectedIndex++;
                            System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.None);
                            System.Windows.Input.Mouse.OverrideCursor = null;
                            System.Windows.Input.Mouse.UpdateCursor();
                        }

                        FilesListView.ScrollIntoView(FilesListView.SelectedItem);
                        if (DataContext is FilesListViewModel viewm && FilesListView.SelectedItem is ItemList sel) viewm.SelectedItem = sel;
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private void GridFileContent_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm)
            {
                // Only act if user is not currently zoomed in
                if (ZoomableImageElement.Visibility == Visibility.Visible && ZoomableImageElement.RenderTransform is TransformGroup group)
                {
                    var scaleTransform = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                    if (scaleTransform != null && scaleTransform.ScaleX > 1.05) return;
                }

                if (VideoMedia.Visibility == Visibility.Visible && VideoMedia.videoInWindowPlayer.RenderTransform is TransformGroup vGroup)
                {
                    var scaleTransform = vGroup.Children.OfType<ScaleTransform>().FirstOrDefault();
                    if (scaleTransform != null && scaleTransform.ScaleX > 1.05) return;
                }

                double totalX = e.TotalManipulation.Translation.X;
                if (Math.Abs(totalX) > 100) // swipe threshold
                {
                    if (totalX > 0 && FilesListView.SelectedIndex > 0)
                    {
                        FilesListView.SelectedIndex--; // swipe right = previous
                    }
                    else if (totalX <= 0 && FilesListView.SelectedIndex < FilesListView.Items.Count - 1)
                    {
                        FilesListView.SelectedIndex++; // swipe left = next
                    }

                    FilesListView.ScrollIntoView(FilesListView.SelectedItem);
                    if (FilesListView.SelectedItem is ItemList sel) vm.SelectedItem = sel;
                    e.Handled = true;
                }
            }
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void FileContentGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ClickCount == 2)
            //{
            try
            {
                if (DataContext is FilesListViewModel vm)
                {
                    if (_filesListViewVisible)
                    {
                        HideUI();
                    }
                    else
                    {
                        ShowUI();
                    }
                }
            }
            catch
            {

            }
        }
        //}

        private void HideUI()
        {
            _filesListColumnWidthCache = FilesListColumn.Width;

            FilesListColumn.Width = new GridLength(0);
            FilesListView.Visibility = Visibility.Collapsed;
            TopInfoPanel.Visibility = Visibility.Collapsed;
            MainWindowGridSplitter.Visibility = Visibility.Collapsed;
            StatusBar.Visibility = Visibility.Collapsed;
            _filesListViewVisible = false;
        }

        private void ShowUI()
        {
            FilesListColumn.Width = _filesListColumnWidthCache;

            TopInfoPanel.Visibility = Visibility.Visible;
            MainWindowGridSplitter.Visibility = Visibility.Visible;
            FilesListView.Visibility = Visibility.Visible;
            StatusBar.Visibility = Visibility.Visible;
            _filesListViewVisible = true;
            FilesListView.ScrollIntoView(FilesListView.SelectedItem);
            FilesListView.IsSynchronizedWithCurrentItem = true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is FilesListViewModel vm &&
                    vm.SelectedItem?.FileContentModel?.ShowTextBox == true &&
                    !string.IsNullOrEmpty(vm.SelectedItem.FullPath))
                {
                    string content = TextBoxTextContent.Text;
                    string filePath = vm.SelectedItem.FullPath;

                    try
                    {
                        // Zapisz zawartość do pliku
                        await System.IO.File.WriteAllTextAsync(filePath, content, System.Text.Encoding.UTF8);

                        // Pokaż potwierdzenie
                        var originalBackground = SaveButton.Background;
                        SaveButton.Content = "Saved!";
                        await System.Threading.Tasks.Task.Delay(1500); // Pokazuj "Saved!" przez 1.5 sekundy
                        SaveButton.Content = "Save";
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(
                            "Access denied. The file may be read-only or you may not have required permissions.",
                            "Save Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    catch (System.IO.IOException ex)
                    {
                        MessageBox.Show(
                            $"Could not save the file: {ex.Message}",
                            "Save Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while saving: {ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private readonly List<int> _searchResults = new List<int>();
        private int _currentSearchIndex = -1;

        private void TextBoxTextContent_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && vm.SelectedItem?.FileContentModel?.IsLargeFileMode == true)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    return; // Let standard zoom logic handle it if implemented elsewhere, or do nothing here
                }

                var textBox = sender as TextBox;
                if (textBox == null) return;

                bool atTop = textBox.VerticalOffset == 0;
                bool atBottom = textBox.VerticalOffset >= textBox.ExtentHeight - textBox.ViewportHeight;

                bool scrollingUp = e.Delta > 0;
                bool scrollingDown = e.Delta < 0;

                // Allow native scrolling within the current chunk if not at boundaries
                if ((scrollingUp && !atTop) || (scrollingDown && !atBottom))
                {
                    return;
                }

                // If we are at the boundaries, load the next/previous chunk
                int chunkSize = (int)Math.Min(vm.Config.CharsToPreview, 65536);
                long offsetChange = scrollingUp ? -(chunkSize / 2) : (chunkSize / 2);
                long newOffset = vm.SelectedItem.FileContentModel.StreamOffset + offsetChange;

                long maxOffset = Math.Max(0, vm.SelectedItem.FileContentModel.FileSize - chunkSize);
                if (newOffset < 0) newOffset = 0;
                if (newOffset > maxOffset) newOffset = maxOffset;

                // Only reload if the offset actually changes
                if (newOffset != vm.SelectedItem.FileContentModel.StreamOffset)
                {
                    LargeFileScrollBar.Value = newOffset;

                    Application.Current.Dispatcher.BeginInvoke(async () =>
                    {
                        await vm.LoadLargeFileChunkAsync(newOffset);

                        // After loading, snap scrollbar to opposite side so the user can continue scrolling smoothly
                        if (scrollingUp)
                            textBox.ScrollToEnd();
                        else
                            textBox.ScrollToHome();
                    });
                }

                e.Handled = true; // Prevent default scrolling bouncing
            }
        }

        private void LargeFileScrollBar_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && vm.SelectedItem?.FileContentModel?.IsLargeFileMode == true)
            {
                Application.Current.Dispatcher.BeginInvoke(async () =>
                {
                    await vm.LoadLargeFileChunkAsync((long)e.NewValue);
                });
            }
        }

        private void SwitchToReadMode_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && vm.SelectedItem != null && vm.SelectedItem.FileContentModel != null)
            {
                // Force load with forceLoad = false, this will use large file mode
                _ = vm.LazyLoadFile(false);
            }
        }

        private void SwitchToEditMode_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && vm.SelectedItem != null && vm.SelectedItem.FileContentModel != null)
            {
                // Force load the entire file into memory and enable edit mode
                _ = vm.LazyLoadFile(true);
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FindNext_Click(sender, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                SearchTextBox.Text = string.Empty;
                SearchResultsCount.Text = string.Empty;
                _searchResults.Clear();
                _currentSearchIndex = -1;
            }
        }

        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text)) return;

            if (_searchResults.Count == 0 || TextBoxTextContent.Text.IndexOf(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase) == -1)
            {
                // New search
                _searchResults.Clear();
                int index = 0;
                while ((index = TextBoxTextContent.Text.IndexOf(SearchTextBox.Text, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    _searchResults.Add(index);
                    index += SearchTextBox.Text.Length;
                }
                _currentSearchIndex = -1;
            }

            if (_searchResults.Count > 0)
            {
                _currentSearchIndex = (_currentSearchIndex + 1) % _searchResults.Count;
                HighlightSearchResult();
            }

            UpdateSearchCount();
        }

        private void FindPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_searchResults.Count == 0 || string.IsNullOrEmpty(SearchTextBox.Text)) return;

            _currentSearchIndex--;
            if (_currentSearchIndex < 0)
                _currentSearchIndex = _searchResults.Count - 1;

            HighlightSearchResult();
            UpdateSearchCount();
        }

        private void HighlightSearchResult()
        {
            if (_currentSearchIndex >= 0 && _currentSearchIndex < _searchResults.Count)
            {
                TextBoxTextContent.Focus();
                TextBoxTextContent.Select(_searchResults[_currentSearchIndex], SearchTextBox.Text.Length);
                TextBoxTextContent.ScrollToLine(TextBoxTextContent.GetLineIndexFromCharacterIndex(_searchResults[_currentSearchIndex]));
            }
        }

        private void UpdateSearchCount()
        {
            if (_searchResults.Count > 0)
                SearchResultsCount.Text = $"{_currentSearchIndex + 1} of {_searchResults.Count} matches";
            else
                SearchResultsCount.Text = "No matches";
        }
        private void StatusBarTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 4)
            {
                var renderCapabilities = typeof(System.Windows.Media.RenderOptions).GetProperty("ProcessRenderMode", BindingFlags.Static | BindingFlags.NonPublic);
                //var tier = System.Windows.Media.RenderCapability.Tier;
                int renderingTier = (RenderCapability.Tier >> 16);
                string maxHardwareTextureSize = $"{RenderCapability.MaxHardwareTextureSize.Height.ToString()} Width: {RenderCapability.MaxHardwareTextureSize.Width.ToString()}";

                MessageBox.Show($"Tier: {renderingTier}\r\nRenderCapabalities: {maxHardwareTextureSize}", "Current Configuration", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void FileFullPathTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (DataContext is QuickViewFile.ViewModel.FilesListViewModel vm && !string.IsNullOrWhiteSpace(FileFullPathTextBox.Text))
                {
                    vm.RefreshFiles(FileFullPathTextBox.Text);
                }
                e.Handled = true;
            }
        }

        private void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is QuickViewFile.ViewModel.FilesListViewModel vm)
            {
                var dialog = new InputDialog("Enter new folder name:");
                dialog.Owner = this;
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Answer))
                {
                    try
                    {
                        string newPath = System.IO.Path.Combine(vm.FolderPath, dialog.Answer);
                        if (!System.IO.Directory.Exists(newPath))
                        {
                            System.IO.Directory.CreateDirectory(newPath);
                            vm.RefreshFiles();
                        }
                        else
                        {
                            MessageBox.Show("Folder already exists.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to create folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CancelPaste_Click(object sender, RoutedEventArgs e)
        {
            _clipboardFiles.Clear();
            _currentOperation = FileOperation.None;
            PasteButton.Visibility = Visibility.Collapsed;
            CancelPasteButton.Visibility = Visibility.Collapsed;
            MoveButton.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            NewFolderButton.Visibility = Visibility.Visible;
        }

        private System.Threading.CancellationTokenSource? _pasteCts;

        private void CancelOperation_Click(object sender, RoutedEventArgs e)
        {
            if (_pasteCts != null)
                _pasteCts.Cancel();
        }

        private int _lastCheckedIndex = -1;

        private void FilesListView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dependencyObject = e.OriginalSource as System.Windows.DependencyObject;
            var listViewItem = FindVisualParent<System.Windows.Controls.ListViewItem>(dependencyObject);

            if (listViewItem != null && DataContext is QuickViewFile.ViewModel.FilesListViewModel vm)
            {
                var clickedData = listViewItem.DataContext as QuickViewFile.Models.ItemList;
                if (clickedData == null) return;

                int currentIndex = vm.ActiveListItems.IndexOf(clickedData);
                if (currentIndex == -1) return;

                if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift) && _lastCheckedIndex != -1)
                {
                    int start = Math.Min(_lastCheckedIndex, currentIndex);
                    int end = Math.Max(_lastCheckedIndex, currentIndex);

                    for (int i = start; i <= end; i++)
                    {
                        vm.ActiveListItems[i].IsChecked = true;
                    }
                    e.Handled = true;
                }
                else
                {
                    _lastCheckedIndex = currentIndex;
                }
            }
        }
}
}