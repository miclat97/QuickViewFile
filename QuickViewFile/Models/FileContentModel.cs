using QuickViewFile.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuickViewFile.Models
{
    public class FileContentModel : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string? _textContent = null;
        public string? TextContent { get => _textContent; set { _textContent = value; OnPropertyChanged(); } }

        public ImageSource? ImageSource { get; set; } = null;

        public VideoPlayerControl? VideoMedia { get; set; } = null;

        public bool IsLoaded { get; set; } = false;
        public bool ShowTextBox { get; set; } = false;

        private bool _showWebView = false;
        public bool ShowWebView { get => _showWebView; set { _showWebView = value; OnPropertyChanged(); } }

        public bool ShowCodeEditor { get; set; } = false;

        private string? _webViewSource = null;
        public string? WebViewSource { get => _webViewSource; set { _webViewSource = value; OnPropertyChanged(); } }

        public void Dispose()
        {
            // Dispose of managed resources.
            TextContent = null;

            if (ImageSource is BitmapSource bitmapSource)
            {
                bitmapSource.Freeze();
            }
            ImageSource = null;

            VideoMedia?.Dispose();
            VideoMedia = null;

            IsLoaded = false;
            ShowTextBox = false;
            ShowWebView = false;
            ShowCodeEditor = false;
            WebViewSource = null;

            // Suppress finalization to prevent GC from calling finalizer on disposed object.
            GC.SuppressFinalize(this);
        }
    }
}
