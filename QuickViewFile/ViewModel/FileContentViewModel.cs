using QuickViewFile.Helpers;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickViewFile.ViewModel
{
    public class FileContentViewModel : INotifyPropertyChanged
    {
        private string? _fileName;
        private string? _textContent;
        private ImageSource? _imageSource;

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

        public ImageSource? ImageSource
        {
            get => _imageSource;
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    OnPropertyChanged(nameof(ImageSource));
                }
            }
        }

        public async Task LoadFileAsync(string? filePath)
        {
            FileName = null;
            TextContent = null;
            ImageSource = null;

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            FileName = Path.GetFileName(filePath);

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif")
            {
                try
                {
                    BitmapImage myBitmapImage = new BitmapImage();
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(filePath);
                    myBitmapImage.EndInit();
                    myBitmapImage.Freeze();
                    ImageSource = myBitmapImage;
                }
                catch (Exception ex)
                {
                    TextContent = ex.Message;
                }
            }
            else
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 50 * 1024)
                {
                    TextContent = "Plik jest zbyt duży do wyświetlenia";
                }
                else
                {
                    TextContent = await FileContentReader.ReadTextFileAsync(filePath)
                        ?? "Nie można odczytać pliku jako tekstowego.";
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}