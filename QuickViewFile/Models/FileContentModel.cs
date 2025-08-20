using System.Windows;
using System.Windows.Media;

namespace QuickViewFile.Models
{
    public class FileContentModel
    {
        public string? TextContent { get; set; } = string.Empty;
        public ImageSource? ImageSource { get; set; } = null;
        public bool IsLoaded { get; set; } = false;
    }
}
