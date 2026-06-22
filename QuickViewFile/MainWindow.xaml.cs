using QuickViewFile;
﻿using QuickViewFile.Controls;
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
            finally
            {
                FilesListView.IsSynchronizedWithCurrentItem = true;
                FilesListView.ScrollIntoView(FilesListView.SelectedItem);
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
            finally
            {
                FilesListView.IsSynchronizedWithCurrentItem = true;
                FilesListView.ScrollIntoView(FilesListView.SelectedItem);
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


        // --- File Operations ---


        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && vm.SelectedItem?.FullPath is not null)
            {
                MainWindowNoBorder fullScreen = new MainWindowNoBorder(vm.SelectedItem.FullPath);
                if (vm.SelectedItem.FileContentModel.VideoMedia is not null)
                {
                    vm.SelectedItem.FileContentModel.VideoMedia.StopForce();
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
        private enum FileOperation { None, Copy, Move }
        private FileOperation _currentOperation = FileOperation.None;
        private List<string> _clipboardFiles = new List<string>();

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

        private void UpdateClipboardFiles()
        {
            var itemsToProcess = GetSelectedOrCheckedItems();
            if (itemsToProcess.Any())
            {
                _clipboardFiles.Clear();
                foreach (ItemList item in itemsToProcess)
                {
                    if (!item.IsDirectory && item.Name != "..")
                    {
                        _clipboardFiles.Add(item.FullPath);
                    }
                }
                if (_clipboardFiles.Count > 0)
                {
                    MoveButton.Visibility = Visibility.Collapsed;
                    CopyButton.Visibility = Visibility.Collapsed;
                    DeleteButton.Visibility = Visibility.Collapsed;
                    PasteButton.Visibility = Visibility.Visible;
                    PasteButton.Content = $"Paste ({_clipboardFiles.Count} files)";
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
                    var result = MessageBox.Show($"Are you sure you want to delete {itemsToDelete.Count} items?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        foreach (ItemList item in itemsToDelete)
                        {
                            if (!item.IsDirectory && item.Name != "..")
                            {
                                try
                                {
                                    File.Delete(item.FullPath);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Failed to delete {item.Name}: {ex.Message}");
                                }
                            }
                        }
                        vm.RefreshFiles();
                    }
                }
            }
        }

        private void PasteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_clipboardFiles.Count > 0 && DataContext is FilesListViewModel vm)
            {
                string targetDir = vm.FolderPath;
                foreach (string file in _clipboardFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(targetDir, fileName);

                        if (_currentOperation == FileOperation.Copy)
                        {
                            File.Copy(file, destFile, true);
                        }
                        else if (_currentOperation == FileOperation.Move)
                        {
                            if (file != destFile)
                            {
                                File.Move(file, destFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Operation failed for {file}: {ex.Message}");
                    }
                }

                if (_currentOperation == FileOperation.Move || _currentOperation == FileOperation.Copy)
                {
                    _clipboardFiles.Clear();
                    _currentOperation = FileOperation.None;
                    PasteButton.Visibility = Visibility.Collapsed;
                    MoveButton.Visibility = Visibility.Visible;
                    CopyButton.Visibility = Visibility.Visible;
                    DeleteButton.Visibility = Visibility.Visible;
                }
                vm.RefreshFiles();
            }
        }

        // --- Column Sorting ---
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

        private void AppWindow_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.F11)
                {
                    if (_filesListViewVisible) HideUI();
                    else ShowUI();
                    e.Handled = true;
                    return;
                }

                if (DataContext is FilesListViewModel vm)
                {
                    if (e.Key == Key.F4 && vm.SelectedItem?.FullPath is not null)
                    {
                        MainWindow fullScreen = new MainWindow(vm.SelectedItem.FullPath);
                        if (vm.SelectedItem.FileContentModel.VideoMedia is not null)
                        {
                            vm.SelectedItem.FileContentModel.VideoMedia.StopForce();
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

                    if (vm.SelectedItem?.FileContentModel.TextContent is not null && isTextFileOpen)
                    {
                        if (e.Key == Key.Escape)
                        {
                            SearchTextBox.Text = string.Empty;
                            SearchResultsCount.Text = string.Empty;
                            _searchResults.Clear();
                            _currentSearchIndex = -1;
                            e.Handled = true;
                            return;
                        }
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
                            if (itemData != null)
                            {
                                itemData.IsChecked = !itemData.IsChecked;

                                int currentIndex = FilesListView.ItemContainerGenerator.IndexFromContainer(focusedElement);
                                int nextIndex = currentIndex + 1;

                                if (nextIndex < FilesListView.Items.Count && nextIndex >= 0)
                                {
                                    // Make sure it also gets focus so next spacebar works without changing the selected item blindly
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        var nextContainer = FilesListView.ItemContainerGenerator.ContainerFromIndex(nextIndex) as ListViewItem;
                                        if (nextContainer != null)
                                        {
                                            nextContainer.Focus();
                                            // Optional: if the user explicitly wants selection to follow spacebar, do it here safely,
                                            // but since checkbox defines multi-select, keep the real selection independent or manage it carefully.
                                            FilesListView.SelectedIndex = nextIndex;
                                            FilesListView.ScrollIntoView(FilesListView.SelectedItem);
                                        }
                                    }), System.Windows.Threading.DispatcherPriority.Background);
                                }
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
                            FilesListView.SelectedIndex++;
                            handled = true;
                        }
                        else if (e.Key == Key.Left || e.Key == Key.Up)
                        {
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
                            if (FilesListView.SelectedIndex < 0) FilesListView.SelectedIndex = 0;
                            if (FilesListView.SelectedIndex >= FilesListView.Items.Count) FilesListView.SelectedIndex = FilesListView.Items.Count - 1;

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



        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && FilesListView.SelectedItem is ItemList selected)
            {
                vm.SelectedItem = selected;
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
            try
            {
                if (DataContext is FilesListViewModel vm)
                {
                    Point mousePosition = e.GetPosition(GridFileContent);

                    double previousItem = GridFileContent.ActualWidth * 0.08;
                    double nextItem = GridFileContent.ActualWidth * 0.92;

                    int nextFileIndex = FilesListView.SelectedIndex + 1;
                    int previousFileIndex = FilesListView.SelectedIndex - 1;

                    if (mousePosition.X < previousItem)
                    {
                        FilesListView.SelectedIndex--;
                        System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.None);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                        System.Windows.Input.Mouse.UpdateCursor();
                    }

                    if (mousePosition.X > nextItem)
                    {
                        FilesListView.SelectedIndex++;
                        System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.None);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                        System.Windows.Input.Mouse.UpdateCursor();
                    }

                    if (FilesListView.SelectedIndex < 0)
                        FilesListView.SelectedIndex = 0;
                    if (FilesListView.SelectedIndex >= FilesListView.Items.Count)
                        FilesListView.SelectedIndex = FilesListView.Items.Count - 1;

                    FilesListView.ScrollIntoView(FilesListView.SelectedItem);
                    if (DataContext is FilesListViewModel viewm && FilesListView.SelectedItem is ItemList sel) viewm.SelectedItem = sel;
                }
            }
            catch (Exception)
            {

            }
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void FileContentGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
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
        }

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

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FindNext_Click(sender, null);
                e.Handled = true;
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
    }
}