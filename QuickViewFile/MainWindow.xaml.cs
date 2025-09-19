using QuickViewFile.Controls;
using QuickViewFile.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuickViewFile
{
    public partial class MainWindow : Window
    {
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

                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        vm.OnFileDoubleClick(file);
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
                    if (e.Key == Key.Enter || e.Key == Key.Space)
                    {
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            vm.OnFileDoubleClick(file);
                        });
                    }
                }
            }
            if (e.Key == Key.Escape)
            {

                Application.Current.Shutdown();
            }
        }

        private void Image_DpiChanged(object sender, DpiChangedEventArgs e)
        {

        }
    }
}