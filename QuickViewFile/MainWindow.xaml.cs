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
            InitializeComponent();
            var vm = new FilesListViewModel(Directory.GetCurrentDirectory());

            DataContext = vm;
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
                            FilesListView.Visibility = Visibility.Hidden;
                            MainWindowGridSplitter.Visibility = Visibility.Hidden;
                            FileFullPathTextBlock.Visibility = Visibility.Hidden;
                            _filesListViewVisible = false;
                        }
                        else
                        {
                            FilesListView.Visibility = Visibility.Visible;
                            MainWindowGridSplitter.Visibility = Visibility.Visible;
                            FileFullPathTextBlock.Visibility = Visibility.Visible;
                            _filesListViewVisible = true;
                            FilesListView.Focus();
                        }
                    }
                }
            }
            catch
            {

            }
        }
    }
}