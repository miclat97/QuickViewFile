using QuickViewFile.Controls;
using QuickViewFile.Helpers;
using QuickViewFile.Models;
using QuickViewFile.ViewModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuickViewFile
{
    public partial class MainWindow : Window
    {
        private bool _filesListViewVisible = true;
        private readonly ConfigModel _config;
        private GridLength _filesListColumnWidthCache;

        public MainWindow()
        {
            try
            {
                _config = ConfigHelper.LoadConfig();

                InitializeComponent();
                FilesListView.Focus();

                string[] args = Environment.GetCommandLineArgs();

                if (!String.IsNullOrWhiteSpace(args.ElementAtOrDefault(1)))
                {
                    string fileToSelectFullPath = args.ElementAt(1);
                    if (File.Exists(fileToSelectFullPath))
                    {
                        FilesListViewModel vm = new FilesListViewModel(fileToSelectFullPath);
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
                        if (e.Key == Key.Right)
                        {
                            FilesListView.SelectedIndex++;
                        }
                        else if (e.Key == Key.Left)
                        {
                            FilesListView.SelectedIndex--;
                        }
                        FilesListView.SetCurrentValue(ListView.SelectedIndexProperty, FilesListView.SelectedIndex);
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
                    }

                    if (vm.SelectedItem?.FileContentModel?.VideoMedia is not null)
                    {
                        if (e.Key == Key.F4)
                        {
                            TimeSpan currentVideoPosition = VideoMedia.GetCurrentVideoPosition();
                            VideoPlayerFullScreen fullScreenVideo = new VideoPlayerFullScreen(vm.SelectedItem.FullPath, _config.BitmapScalingMode, currentVideoPosition);
                            vm.SelectedItem = null;
                            fullScreenVideo.Show();
                        }
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

            _filesListViewVisible = false;
        }

        private void ShowUI()
        {
            FilesListColumn.Width = _filesListColumnWidthCache;

            TopInfoPanel.Visibility = Visibility.Visible;
            MainWindowGridSplitter.Visibility = Visibility.Visible;
            FilesListView.Visibility = Visibility.Visible;
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
                        FilesListView.SelectedIndex--;

                    if (mousePosition.X > nextItem)
                        FilesListView.SelectedIndex++;
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
            if (e.Key > Key.D0 && e.Key < Key.Z)
            {
                char ASCIINumberWhichUserWantToSelect = (char)((int)e.Key + 21); ///because ASCII at keyboard has code 65, in .NET A on keyboard is 44, so after adding 21 it will be this letter which user want to find

                var itemToSelect = FilesListView.Items.Cast<QuickViewFile.Models.ItemList>()
                    .FirstOrDefault(item => !string.IsNullOrEmpty(item.Name) && char.ToUpper(item.Name[0]) == ASCIINumberWhichUserWantToSelect);
                if (itemToSelect is not null)
                {
                    FilesListView.SelectedItem = itemToSelect;
                    FilesListView.ScrollIntoView(itemToSelect);
                }
            }
            if (e.Key == Key.Enter)
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
        }
    }
}