using QuickViewFile.Helpers;
using QuickViewFile.Models;
using System.ComponentModel;
using System.IO;
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

        public async Task LoadTextFileAsync(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                FileName = null;
                TextContent = null;
                return;
            }

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > 50 * 1024)
            {
                FileName = Path.GetFileName(filePath);
                TextContent = "Plik jest zbyt duży do wyświetlenia.";
                return;
            }

            FileName = Path.GetFileName(filePath);
            TextContent = await FileContentReader.ReadTextFileAsync(filePath)
                ?? "Nie można odczytać pliku jako tekstowego.";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
