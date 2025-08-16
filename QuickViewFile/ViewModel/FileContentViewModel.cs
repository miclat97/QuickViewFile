using QuickViewFile.Models;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QuickViewFile.ViewModel
{
    public class FileContentViewModel : INotifyPropertyChanged
    {
        private string? _fileName;
        private string? _textContent;

        public string? FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        public string? TextContent
        {
            get => _textContent;
            set
            {
                if (_textContent != value)
                {
                    _textContent = value;
                    OnPropertyChanged(nameof(TextContent));
                }
            }
        }

        public async Task LoadTextFileAsync(string? filePath, long fileSize)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) || fileSize > 50 * 1024) // 50 KB limit
            {
                FileName = null;
                TextContent = null;
                return;
            }
            else
            {
                try
                {
                    FileName = Path.GetFileName(filePath);
                    var content = await File.ReadAllTextAsync(filePath);
                    TextContent = content;
                }
                catch
                {
                    throw new IOException("Error reading the file.");
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
