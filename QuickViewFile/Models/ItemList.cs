using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuickViewFile.Models
{
    public class ItemList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }
        public string? Name { get; set; } // Name of the file with extension
        public string? Size { get; set; } // File size in bytes
        public string? FullPath { get; set; } // Full path of the file to preview
        public bool IsDirectory { get; set; } // Indicates if the item is a directory
        public FileContentModel FileContentModel { get; set; } = new FileContentModel(); // Lazy loaded file
        public System.DateTime LastModified { get; set; }
        public string LastModifiedString { get; set; }
        public double SizeBytes { get; set; }
        public bool IsAlternativeDataStream { get; set; } = false; // Indicates if the item is an alternative data stream
        public bool IsCheckBoxEnabled => !IsAlternativeDataStream;

        private System.Windows.Media.ImageSource? _thumbnailImageSource;
        public System.Windows.Media.ImageSource? ThumbnailImageSource
        {
            get => _thumbnailImageSource;
            set { _thumbnailImageSource = value; OnPropertyChanged(); }
        }

        private System.Uri? _thumbnailVideoSource;
        public System.Uri? ThumbnailVideoSource
        {
            get => _thumbnailVideoSource;
            set { _thumbnailVideoSource = value; OnPropertyChanged(); }
        }

        private string? _thumbnailTextPreview;
        public string? ThumbnailTextPreview
        {
            get => _thumbnailTextPreview;
            set { _thumbnailTextPreview = value; OnPropertyChanged(); }
        }

        private bool _isVideoThumbnail;
        public bool IsVideoThumbnail
        {
            get => _isVideoThumbnail;
            set { _isVideoThumbnail = value; OnPropertyChanged(); }
        }

        public async System.Threading.Tasks.Task LoadThumbnailAsync(ConfigModel config)
        {
            if (IsDirectory || string.IsNullOrEmpty(FullPath)) return;

            string ext = System.IO.Path.GetExtension(FullPath)?.ToLowerInvariant() ?? "";

            var imageExtensions = Helpers.ConfigHelper.GetStringsFromCommaSeparatedString(config.ImageExtensions);
            var videoExtension = Helpers.ConfigHelper.GetStringsFromCommaSeparatedString(config.VideoExtensions);

            if (videoExtension.Contains(ext))
            {
                IsVideoThumbnail = true;
                ThumbnailVideoSource = new System.Uri(FullPath);
            }
            else if (imageExtensions.Contains(ext))
            {
                try
                {
                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        var bitmap = Helpers.LoadImageWithOrientationHelper.LoadImageWithOrientation(FullPath);
                        if (bitmap != null)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                ThumbnailImageSource = bitmap;
                            });
                        }
                    });
                }
                catch { }
            }
            else
            {
                // Text or binary
                if (SizeBytes <= config.MaxSizePreviewKB * 1024)
                {
                    try
                    {
                        string text = await System.Threading.Tasks.Task.Run(() => FileTextExtractor.GetCleanTextFast(FullPath));
                        ThumbnailTextPreview = text;
                    }
                    catch
                    {
                        ThumbnailTextPreview = "Cannot read file";
                    }
                }
                else
                {
                    ThumbnailTextPreview = $"File is larger than {config.MaxSizePreviewKB} KB";
                }
            }
        }
    }
}
