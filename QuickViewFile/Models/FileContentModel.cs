using QuickViewFile.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickViewFile.Models
{
    public class FileContentModel : IDisposable
    {
        public string? TextContent { get; set; } = null;

        public ImageSource? ImageSource { get; set; } = null;

        public VideoPlayerControl? VideoMedia { get; set; } = null;

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
