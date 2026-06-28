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
        public bool IsCheckBoxEnabled => !IsDirectory || Name == "..";
    }
}
