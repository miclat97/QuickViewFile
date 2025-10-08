using QuickViewFile.Controls;
using QuickViewFile.Models;
using QuickViewFile.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace QuickViewFile
{
    public partial class MainWindow : Window
    {
        private bool _filesListViewVisible = true;
        private readonly ConfigModel _config;

        public MainWindow()
        {
            try
            {
                _config = ConfigHelper.LoadConfig();

                InitializeComponent();
                FilesListView.Focus();

                string[] args = Environment.GetCommandLineArgs();

                if (args.ElementAtOrDefault(1) is not null)
                {
                    var fileToSelectFullPath = args.ElementAt(1);
                    if (File.Exists(fileToSelectFullPath))
                    {
                        var vm = new FilesListViewModel(fileToSelectFullPath);
                        DataContext = vm;
                    }
                }
                else
                {
                    var vm = new FilesListViewModel(Directory.GetCurrentDirectory());
                    DataContext = vm;

                }
            }
            catch
            {
                var vm = new FilesListViewModel(Directory.GetCurrentDirectory());
                DataContext = vm;
            }
            finally
            {
                FilesListView.Focus();
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
                        WindowState = WindowState.Maximized;
                    }
                    else
                    {
                        ShowUI();
                        WindowState = WindowState.Normal;
                    }
                }

                if (DataContext is FilesListViewModel vm)
                {
                    if (sender is ListView listView && listView.SelectedItem is QuickViewFile.Models.ItemList file)
                    {
                        if (e.Key > Key.D0 && e.Key < Key.Z)
                        {
                            char ASCIINumberWhichUserWantToSelect = (char)((int)e.Key + 21); ///because ASCII at keyboard has code 65, in .NET A on keyboard is 44, so after adding 21 it will be this letter which user want to find
                            var itemToSelect = vm.ActiveListItems.FirstOrDefault
                                (x => x.Name?.ToUpperInvariant().ElementAt(0) == ASCIINumberWhichUserWantToSelect);
                            if (itemToSelect is not null)
                            {
                                FilesListView.SelectedItem = itemToSelect;
                                FilesListView.ScrollIntoView(itemToSelect);
                            }
                        }

                        if (e.Key == Key.Enter)
                        {
                            if (file.IsDirectory)
                            {
                                Application.Current.Dispatcher.BeginInvoke(async () =>
                                {
                                    await vm.OnFileDoubleClick(file);
                                });
                            }
                            else if (file.IsDirectory == false && file.FileContentModel.IsLoaded == false)
                            {
                                Application.Current.Dispatcher.BeginInvoke(async () =>
                                {
                                    await vm.LazyLoadFile(true);
                                });
                            }
                        }
                    }

                    if (!_filesListViewVisible) // When UI is hidden and user click anything on keyboard it have to be different implementation due to "standard/Windows"
                                                // handling keyboard - like arrow up or arrow down, when focused on list will change element to previous/net
                    {
                        if (vm.SelectedItem?.FileContentModel?.VideoMedia is null) // when video is playing - arrows are handled to change video time position
                        {
                            int nextFileIndex = FilesListView.SelectedIndex + 1;
                            int previousFileIndex = FilesListView.SelectedIndex - 1;

                            if (e.Key == Key.Right && vm.ActiveListItems.ElementAt(nextFileIndex).IsDirectory == false)
                            {
                                FilesListView.SelectedIndex++;
                            }
                            else if (e.Key == Key.Left && vm.ActiveListItems.ElementAt(previousFileIndex).IsDirectory == false)
                            {
                                FilesListView.SelectedIndex--;
                            }
                            else if (e.Key == Key.Enter)
                            {
                                Application.Current.Dispatcher.BeginInvoke(async () =>
                                {
                                    await vm.LazyLoadFile(true);
                                });
                            }
                        }
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
            FilesListView.Visibility = Visibility.Collapsed;
            MainWindowGridSplitter.Visibility = Visibility.Collapsed;
            FileFullPathTextBlock.Visibility = Visibility.Collapsed;
            _filesListViewVisible = false;
        }

        private void ShowUI()
        {
            FilesListView.Visibility = Visibility.Visible;
            MainWindowGridSplitter.Visibility = Visibility.Visible;
            FileFullPathTextBlock.Visibility = Visibility.Visible;
            _filesListViewVisible = true;
            FilesListView.Focus();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.OriginalSource is Border)
                {
                    if (DataContext is FilesListViewModel vm)
                    {
                        if (vm.SelectedItem?.FileContentModel.ImageSource is not null || vm.SelectedItem?.FileContentModel.VideoMedia is not null)
                        {
                            Point mousePosition = e.GetPosition(ContentBorder);

                            double previousItem = ContentBorder.ActualWidth * 0.08;
                            double nextItem = ContentBorder.ActualWidth * 0.92;

                            int nextFileIndex = FilesListView.SelectedIndex + 1;
                            int previousFileIndex = FilesListView.SelectedIndex - 1;

                            if ((vm.ActiveListItems.ElementAtOrDefault(previousFileIndex) is not null
                                && mousePosition.X < previousItem) &&
                                vm.ActiveListItems.ElementAt(previousFileIndex).IsDirectory == false) // to prevent changing to left from item at position 0 (but it shouldn't crash the app anyway)
                                FilesListView.SelectedIndex--;

                            if ((mousePosition.X > nextItem) &&
                                vm.ActiveListItems.ElementAt(nextFileIndex).IsDirectory == false
                                && vm.ActiveListItems.ElementAt(nextFileIndex) is not null) // to prevent situation when We will try to check ElementAt poisiton out of list (when last photo of directory will be clicked at the right bound (doing so will crash whole appication)
                                FilesListView.SelectedIndex++;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
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

        private List<int> _searchResults = new List<int>();
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
    }
}