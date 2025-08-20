using QuickViewFile.Helpers;
using QuickViewFile.Models;
using QuickViewFile.Watchers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace QuickViewFile.ViewModel
{
    public class FilesListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ItemList> ActiveListItems { get; set; } = new();

        public FilesListViewModel(string folderPath)
        {
            _folderPath = folderPath;
            RefreshFiles();
            _folderWatcher = new FolderWatcher(_folderPath);
            _folderWatcher.OnFolderChanged += RefreshFiles;
        }

        private ItemList? _selectedItem;
        private readonly FolderWatcher _folderWatcher;
        private string _folderPath = Directory.GetCurrentDirectory();
        private double _windowWidth;
        private double _windowHeight;

        public double WindowWidth
        {
            get => _windowWidth;
            set
            {
                if (_windowWidth != value)
                {
                    _windowWidth = value;
                    OnPropertyChanged(nameof(WindowWidth));
                }
            }
        }

        public double WindowHeight
        {
            get => _windowHeight;
            set
            {
                if (_windowHeight != value)
                {
                    _windowHeight = value;
                    OnPropertyChanged(nameof(WindowHeight));
                }
            }
        }

        public double PreviewWidth => WindowWidth * 0.55;
        public double PreviewHeight => WindowHeight * 0.8;


        public ItemList? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    SelectedFileChanged?.Invoke(_selectedItem);

                    if (SelectedItem?.FullPath is not null && !SelectedItem.IsDirectory)
                    {
                        try
                        {
                            this.LazyLoadFile(false);
                        }
                        catch (Exception ex)
                        {
                            SelectedItem.FileContentModel.TextContent = $"{ex.Message}";
                            SelectedItem.FileContentModel.ImageSource = null;
                        }
                    }
                }
            }
        }

        public event System.Action<ItemList?>? SelectedFileChanged;

        private void RefreshFiles()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(_folderPath);

            DirectoryInfo[] foldersInDirectory;
            FileInfo[] filesInDirectory;

            try
            {
                foldersInDirectory = dirInfo.GetDirectories();
                filesInDirectory = dirInfo.GetFiles();
            }
            catch
            {
                Application.Current.Dispatcher.BeginInvoke(() => ActiveListItems.Clear());
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                ActiveListItems.Clear();

                if (dirInfo.Parent != null)
                {
                    ActiveListItems.Add(new ItemList
                    {
                        Name = "..",
                        Size = "",
                        FullPath = dirInfo.Parent.FullName,
                        IsDirectory = true,
                        FileContentModel = new FileContentModel()
                    });
                }

                foreach (var folder in foldersInDirectory)
                {
                    ActiveListItems.Add(new ItemList
                    {
                        Name = folder.Name,
                        Size = "",
                        FullPath = folder.FullName,
                        IsDirectory = true,
                        FileContentModel = new FileContentModel()
                    });
                }

                foreach (var file in filesInDirectory)
                {
                    ActiveListItems.Add(new ItemList
                    {
                        Name = file.Name,
                        Size = (file.Length / 1024).ToString(),
                        FullPath = file.FullName,
                        IsDirectory = false,
                        FileContentModel = new FileContentModel()
                    });
                }
            });
        }

        public void OnFileDoubleClick(ItemList? file)
        {
            if (file == null)
                return;

            if (file.IsDirectory && Directory.Exists(file.FullPath))
            {
                _folderPath = file.FullPath!;
                RefreshFiles();
                SelectedItem = null;
            }
            else if (!file.IsDirectory && File.Exists(file.FullPath))
            {
                SelectedItem = file;
                App.Current.Dispatcher.BeginInvoke(() =>
                {
                    SelectedItem.FileContentModel = new FileContentModel
                    {
                        TextContent = "Ładowanie pliku...",
                        ImageSource = null
                    };
                    this.LazyLoadFile(true);
                });
            }
            else
            {
                _folderPath = Directory.GetCurrentDirectory();
                MessageBox.Show("Wybrany plik lub folder nie istnieje bądź brak uprawnień do odczytu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                SelectedItem = null;
            }
        }

        public void LazyLoadFile(bool? forceLoad = false)
        {
            if (SelectedItem == null || string.IsNullOrWhiteSpace(SelectedItem.FullPath) || !File.Exists(SelectedItem.FullPath))
                return;

            var filePath = SelectedItem.FullPath;
            SelectedItem.FileContentModel = new FileContentModel();

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            bool isImage = ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".webp" || ext == ".avif" || ext == ".gif";
            var fileInfo = new FileInfo(filePath);

            if (isImage)
            {
                try
                {
                    BitmapImage myBitmapImage = new BitmapImage();
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(filePath);
                    myBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    myBitmapImage.EndInit();
                    myBitmapImage.Freeze();
                    SelectedItem.FileContentModel.ImageSource = myBitmapImage;
                }
                catch (Exception ex)
                {
                    SelectedItem.FileContentModel.TextContent = ex.Message;
                }
            }
            else
            {
                if (fileInfo.Length < 50 * 1024 || forceLoad == true)
                {
                    var loadedFileText = FileContentReader.ReadTextFile(filePath);
                    SelectedItem.FileContentModel.TextContent = loadedFileText;
                    SelectedItem.FileContentModel.ImageSource = null;
                }
                else
                {
                    SelectedItem.FileContentModel.TextContent = "Plik jest duży, wciśnij ENTER aby załadować jego zawartość";
                }
            }

            OnPropertyChanged(nameof(SelectedItem));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}