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

        private void AppWindow_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.F11)
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

                if (DataContext is FilesListViewModel vm)
                {
                    try
                    {
                        if (e.Key == Key.Right || e.Key == Key.Down)
                        {
                            FilesListView.SelectedIndex++;
                        }
                        else if (e.Key == Key.Left || e.Key == Key.Up)
                        {
                            FilesListView.SelectedIndex--;
                        }
                        else if (e.Key == Key.PageDown)
                        {
                            if (FilesListView.SelectedIndex + 10 >= FilesListView.Items.Count)
                            {
                                FilesListView.SelectedIndex = FilesListView.Items.Count - 1;
                            }
                            else
                            {
                                FilesListView.SelectedIndex += 10;
                            }
                        }
                        else if (e.Key == Key.PageUp)
                        {
                            if (FilesListView.SelectedIndex - 10 < 0)
                            {
                                FilesListView.SelectedIndex = 0;
                            }
                            else
                            {
                                FilesListView.SelectedIndex -= 10;
                            }
                        }
                        if (e.Key == Key.Home)
                        {
                            FilesListView.SelectedIndex = 0;
                        }
                        else if (e.Key == Key.End)
                        {
                            FilesListView.SelectedIndex = FilesListView.Items.Count - 1;
                        }

                        if (FilesListView.SelectedIndex < 0)
                        {
                            FilesListView.SelectedIndex = 0;
                        }
                        else if (FilesListView.SelectedIndex >= FilesListView.Items.Count)
                        {
                            FilesListView.SelectedIndex = FilesListView.Items.Count - 1;
                        }
                        FilesListView.SetCurrentValue(ListView.SelectedIndexProperty, FilesListView.SelectedIndex);
                        FilesListView.ScrollIntoView(FilesListView.SelectedItem);
                    }
                    catch (Exception)
                    {

                    }

                    if (vm.SelectedItem?.FileContentModel?.ShowTextBox == true)
                    {
                        if (e.Key == Key.Add)
                        {
                            TextBoxTextContent.FontSize += 0.5;
                        }
                        else if (e.Key == Key.Subtract)
                        {
                            if (TextBoxTextContent.FontSize >= 1)
                            {
                                TextBoxTextContent.FontSize -= 0.5;
                            }
                        }
                        ConfigHelper.SetFontSize(TextBoxTextContent.FontSize);
                    }

                    if (e.Key == Key.F4 && vm.SelectedItem?.FullPath is not null)
                    {
                        MainWindowNoBorder fullScreen = new MainWindowNoBorder(vm.SelectedItem.FullPath);
                        if (vm.SelectedItem.FileContentModel.VideoMedia is not null)
                        {
                            vm.SelectedItem.FileContentModel.VideoMedia.StopForce();
                        }
                        fullScreen.Show();
                        this.Close();
                    }

                    if (vm.SelectedItem?.FileContentModel.TextContent is not null)
                    {
                        if (e.Key == Key.Escape)
                        {
                            SearchTextBox.Text = string.Empty;
                            SearchResultsCount.Text = string.Empty;
                            _searchResults.Clear();
                            _currentSearchIndex = -1;
                            e.Handled = true;
                        }
                    }

                    if (e.Key == Key.Multiply)
                    {
                        degreesRotation += 90;
                        if (degreesRotation < 360)
                        {
                            GridFileContent.LayoutTransform = new RotateTransform(degreesRotation);
                        }
                        else
                        {
                            degreesRotation = 0;
                            GridFileContent.LayoutTransform = new RotateTransform(0);
                        }
                    }

                    if (e.Key >= Key.A && e.Key <= Key.Z)
                    {
                        char ASCIINumberWhichUserWantToSelect = e.Key.ToString()[0];

                        var nextItems = new ItemList();

                        var itemToSelect = FilesListView.Items.Cast<QuickViewFile.Models.ItemList>().Skip(FilesListView.SelectedIndex + 1)
                            .FirstOrDefault(item => !string.IsNullOrEmpty(item.Name) && char.ToUpper(item.Name[0]) == ASCIINumberWhichUserWantToSelect);

                        if (itemToSelect is null)                        //search from beginning if not found in next items
                        {
                            itemToSelect = FilesListView.Items.Cast<QuickViewFile.Models.ItemList>()
                                .FirstOrDefault(item => !string.IsNullOrEmpty(item.Name) && char.ToUpper(item.Name[0]) == ASCIINumberWhichUserWantToSelect);
                        }
                        if (itemToSelect is not null)
                        {
                            FilesListView.SelectedItem = itemToSelect;
                            FilesListView.ScrollIntoView(itemToSelect);
                        }
                    }

                    if (e.Key == Key.Enter)
                    {
                        Application.Current.Dispatcher.BeginInvoke(async () =>
                        {
                            await vm.OnFileDoubleClick(vm.SelectedItem);
                        });
                    }
                }
            }
            catch
            {

            }
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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
                        Mouse.SetCursor(Cursors.None);
                        Mouse.OverrideCursor = null;
                        Mouse.UpdateCursor();
                    }

                    if (mousePosition.X > nextItem)
                    {
                        FilesListView.SelectedIndex++;
                        Mouse.SetCursor(Cursors.None);
                        Mouse.OverrideCursor = null;
                        Mouse.UpdateCursor();
                    }

                    if (FilesListView.SelectedIndex < 0)
                        FilesListView.SelectedIndex = 0;
                    if (FilesListView.SelectedIndex >= FilesListView.Items.Count)
                        FilesListView.SelectedIndex = FilesListView.Items.Count - 1;

                    FilesListView.ScrollIntoView(FilesListView.SelectedItem);
                }
            }
            catch (Exception)
            {

            }
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
                        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);

                        // Pokaż potwierdzenie
                        var originalBackground = SaveButton.Background;
                        SaveButton.Content = "Saved!";
                        await Task.Delay(1500); // Pokazuj "Saved!" przez 1.5 sekundy
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
                    catch (IOException ex)
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

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
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

        private readonly List<int> _searchResults = [];
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

        private void FilesListView_KeyDown(object sender, KeyEventArgs e)
        {

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