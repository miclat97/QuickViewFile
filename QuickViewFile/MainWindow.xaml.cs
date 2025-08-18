using QuickViewFile.Models;
using QuickViewFile.ViewModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
                            // Call the method to handle double-click action
                            vm.OnFileDoubleClick(file).Wait();
                        });
                    }
                }
            }
        }
    }
}