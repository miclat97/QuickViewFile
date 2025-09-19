using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuickViewFile.Controls
{
    /// <summary>
    /// Interaction logic for VideoMediaWindow.xaml
    /// </summary>
    public partial class VideoMediaWindow : Window
    {
        public VideoMediaWindow(Uri fileUri)
        {
            InitializeComponent();
            VideoMediaWindow_MediaElement.Source = fileUri;
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            VideoMediaWindow_MediaElement.Close();
            this.Close();
        }
    }
}
