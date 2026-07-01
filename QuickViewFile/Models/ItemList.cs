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
            set
            {
                _thumbnailImageSource = value;
                OnPropertyChanged(nameof(ThumbnailImageSource));
            }
        }

        private string? _thumbnailTextPreview;
        public string? ThumbnailTextPreview
        {
            get => _thumbnailTextPreview;
            set
            {
                _thumbnailTextPreview = value;
                OnPropertyChanged(nameof(ThumbnailTextPreview));
            }
        }

        private Uri? _thumbnailVideoSource;
        public Uri? ThumbnailVideoSource
        {
            get => _thumbnailVideoSource;
            set
            {
                _thumbnailVideoSource = value;
                OnPropertyChanged(nameof(ThumbnailVideoSource));
                OnPropertyChanged(nameof(IsVideoThumbnail));
            }
        }

        public bool IsVideoThumbnail => ThumbnailVideoSource != null;

        public async System.Threading.Tasks.Task LoadThumbnailAsync(ConfigModel config, System.Threading.CancellationToken cancellationToken)
        {
            if (IsDirectory || Name == "..") return;

            string filePath = FullPath;
            string extensionPath = filePath;
            int colonIndex = extensionPath.LastIndexOf(':');
            if (colonIndex > 3)
            {
                extensionPath = extensionPath.Substring(0, colonIndex);
            }

            string ext = System.IO.Path.GetExtension(extensionPath).ToLowerInvariant();

            var imageExtensions = QuickViewFile.Helpers.ConfigHelper.GetStringsFromCommaSeparatedString(config.ImageExtensions);
            var musicExtensions = QuickViewFile.Helpers.ConfigHelper.GetStringsFromCommaSeparatedString(config.MusicExtensions);
            var videoExtension = QuickViewFile.Helpers.ConfigHelper.GetStringsFromCommaSeparatedString(config.VideoExtensions);

            if (cancellationToken.IsCancellationRequested) return;

            if (imageExtensions.Contains(ext))
            {
                try
                {
                    var source = await System.Threading.Tasks.Task.Run(() => QuickViewFile.Helpers.LoadImageWithOrientationHelper.LoadImageWithOrientation(filePath), cancellationToken);
                    if (!cancellationToken.IsCancellationRequested) ThumbnailImageSource = source;
                }
                catch { }
            }
            else if (videoExtension.Contains(ext))
            {
                if (!cancellationToken.IsCancellationRequested) ThumbnailVideoSource = new Uri(filePath);
            }
            else if (!musicExtensions.Contains(ext))
            {
                try
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                    if (fileInfo.Length < config.MaxSizePreviewKB * 1024)
                    {
                        var text = await System.IO.File.ReadAllTextAsync(filePath, cancellationToken);
                        if (text.Length > 200) text = text.Substring(0, 200) + "...";
                        if (!cancellationToken.IsCancellationRequested) ThumbnailTextPreview = text;
                    }
                    else
                    {
                        if (!cancellationToken.IsCancellationRequested) ThumbnailTextPreview = $"File is larger than {config.MaxSizePreviewKB} KB";
                    }
                }
                catch { }
            }
        }
    }
}
