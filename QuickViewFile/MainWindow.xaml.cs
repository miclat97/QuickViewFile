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
            this.SizeChanged += (s, e) =>
            {
                vm.WindowWidth = this.ActualWidth;
                vm.WindowHeight = this.ActualHeight;
            };
            
            vm.WindowWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            vm.WindowHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
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

        private void FilesListView_KeyDown(object sender, KeyEventArgs e) //Change directory or force load file (using keyboard)
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
        }

        private void Image_DpiChanged(object sender, DpiChangedEventArgs e)
        {

        }
    }
}