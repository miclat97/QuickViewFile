using QuickViewFile.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuickViewFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new FilesListViewModel(Directory.GetCurrentDirectory());
        }

        private void FilesListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) //Change directory
        {
            if (DataContext is FilesListViewModel vm)
            {
                if (sender is ListView listView && listView.SelectedItem is QuickViewFile.Models.ItemList file)
                {

                    Dispatcher.Invoke(() =>
                    {
                        vm.OnFileDoubleClick(file).Wait();
                    });
                }
            }
        }

        private void FilesListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is FilesListViewModel vm)
            {
                if (sender is ListView listView && listView.SelectedItem is QuickViewFile.Models.ItemList file)
                {
                    if (e.Key == Key.Enter || e.Key == Key.Space)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            vm.OnFileDoubleClick(file).Wait();
                        });
                    }
                }
            }
        }
    }
}