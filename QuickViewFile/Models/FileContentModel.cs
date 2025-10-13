using QuickViewFile.Controls;
using System.Windows.Media;
using System;
using System.Windows.Media.Imaging;

namespace QuickViewFile.Models
{
    public class FileContentModel : IDisposable
    {
        private string? _textContent = null;
        private ImageSource? _imageSource = null;
        private VideoPlayerControl? _videoMedia = null;

        public string? TextContent
        {
            get => _textContent;
            set { _textContent = value; }
        }

        public ImageSource? ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; }
        }

        public VideoPlayerControl? VideoMedia
        {
            get => _videoMedia;
            set { _videoMedia = value; }
        }

        public bool IsLoaded { get; set; } = false;
        public bool ShowTextBox { get; set; } = false;

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

            // Suppress finalization to prevent GC from calling finalizer on disposed object.
            GC.SuppressFinalize(this);
        }
    }
}
