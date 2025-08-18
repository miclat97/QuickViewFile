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

        private ItemList? _selectedItem;
        private readonly FolderWatcher _folderWatcher;
        private string _folderPath = Directory.GetCurrentDirectory();

        public FilesListViewModel(string folderPath)
        {
            _folderPath = folderPath;
            RefreshFiles();
            _folderWatcher = new FolderWatcher(_folderPath);
            _folderWatcher.OnFolderChanged += RefreshFiles;
        }

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
                    _ = LazyLoadFileAsync(false);
                }
            }
        }

        public event System.Action<ItemList?>? SelectedFileChanged;

        private async void RefreshFiles()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(_folderPath);

            DirectoryInfo[] foldersInDirectory;
            FileInfo[] filesInDirectory;

            try
            {
                (foldersInDirectory, filesInDirectory) = await Task.Run(() =>
                {
                    var folders = dirInfo.GetDirectories();
                    var files = dirInfo.GetFiles();
                    return (folders, files);
                });
            }
            catch
            {
                App.Current.Dispatcher.Invoke(() => ActiveListItems.Clear());
                return;
            }

            App.Current.Dispatcher.Invoke(() =>
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

        public async Task OnFileDoubleClick(ItemList? file)
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
                await LazyLoadFileAsync(true);
            }
            else
            {
                _folderPath = Directory.GetCurrentDirectory();
                MessageBox.Show("Wybrany plik lub folder nie istnieje bądź brak uprawnień do odczytu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                SelectedItem = null;
            }
        }

        public async Task LazyLoadFileAsync(bool? forceLoad = false)
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
                    SelectedItem.FileContentModel.TextContent = await FileContentReader.ReadTextFileAsync(filePath);
                }
                else
                {
                    SelectedItem.FileContentModel.TextContent = "Plik jest zbyt duży aby automatycznie wyświetlić jego tekstowy podgląd. Możesz wymusić załadowanie jego zawartości przez dwukrotne kliknięcie na plik, bądź naciśnięcie ENTER na klawiaturze";
                }
            }

            // Notify UI about the change
            OnPropertyChanged(nameof(SelectedItem));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}