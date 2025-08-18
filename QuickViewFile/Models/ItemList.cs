namespace QuickViewFile.Models
{
    public class ItemList
    {
        public string? Name { get; set; } // Name of the file with extension
        public string? Size { get; set; } // File size in bytes
        public string? FullPath { get; set; } // Full path of the file to preview
        public bool IsDirectory { get; set; } // Indicates if the item is a directory
        public FileContentModel FileContentModel { get; set; } = new FileContentModel(); // Lazy loaded file
    }
}
