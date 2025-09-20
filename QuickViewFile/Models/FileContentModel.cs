using QuickViewFile.Controls;
using System.Windows.Media;

namespace QuickViewFile.Models
{
    public class FileContentModel : IDisposable
    {
        public string? TextContent { get; set; } = string.Empty;
        public ImageSource? ImageSource { get; set; } = null;
        public VideoPlayerControl? VideoMedia { get; set; } = null;
        public bool IsLoaded { get; set; } = false;

        public void Dispose()
        {
            this.TextContent = null;
            this.ImageSource = null;
            this.VideoMedia?.Dispose();
            this.IsLoaded = false;
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
