using QuickViewFile.Controls;
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
        public MainWindow()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();

                if (args.ElementAtOrDefault(1) is not null)
                {
                    var vm = new FilesListViewModel(Path.Combine(args.ElementAt(1)));
                    DataContext = vm;
                }
                else
                {
                    var vm = new FilesListViewModel(Directory.GetCurrentDirectory());
                    DataContext = vm;
                }
                InitializeComponent();
            }
            catch
            {
                var vm = new FilesListViewModel(Directory.GetCurrentDirectory());
                DataContext = vm;
                InitializeComponent();
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
            if (DataContext is FilesListViewModel vm)
            {
                if (sender is ListView listView && listView.SelectedItem is QuickViewFile.Models.ItemList file)
                {
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

                if (TextBoxTextContent.Text is not null)
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
            }
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DataContext is FilesListViewModel vm)
                {
                    if (vm.SelectedItem?.FileContentModel.VideoMedia is not null)
                    {
                        VideoPlayerFullScreen fullScreenVideo = new VideoPlayerFullScreen(vm.SelectedItem.FullPath);
                        vm.SelectedItem = null;
                        fullScreenVideo.Show();
                    }
                    else
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
            }
            catch
            {

            }
        }

        private void HideUI()
        {
            FilesListView.Visibility = Visibility.Collapsed;
            MainWindowGridSplitter.Visibility = Visibility.Collapsed;
            FileFullPathTextBlock.Visibility = Visibility.Hidden;
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
                            && mousePosition.X < previousItem) && vm.ActiveListItems.ElementAt(previousFileIndex).IsDirectory == false) // to prevent changing to left from item at position 0 (but it shouldn't crash the app anyway)
                            FilesListView.SelectedIndex--;

                        if ((mousePosition.X > nextItem) && vm.ActiveListItems.ElementAt(nextFileIndex).IsDirectory == false
                            && vm.ActiveListItems.ElementAtOrDefault(nextFileIndex) is not null) // to prevent situation when We will try to check ElementAt poisiton out of list (when last photo of directory will be clicked at the right bound (doing so will crash whole appication)
                            FilesListView.SelectedIndex++;
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
    }
}