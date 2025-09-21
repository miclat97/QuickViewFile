using QuickViewFile.Helpers;
using QuickViewFile.Models;
using QuickViewFile.Watchers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            Config = ConfigHelper.LoadConfig();
            PreviewHeight = Config.PreviewHeight;
            PreviewWidth = Config.PreviewWidth;
        }

        private ItemList? _selectedItem;
        private readonly FolderWatcher _folderWatcher;
        private string _folderPath = Directory.GetCurrentDirectory();

        public ConfigModel Config { get; set; } = ConfigHelper.LoadConfig();



        private double _previewHeight;
        public double PreviewHeight
        {
            get => _previewHeight;
            set { _previewHeight = value; OnPropertyChanged(nameof(PreviewHeight)); }
        }

        private double _previewWidth;
        public double PreviewWidth
        {
            get => _previewWidth;
            set { _previewWidth = value; OnPropertyChanged(nameof(PreviewWidth)); }
        }


        public ItemList? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem?.FileContentModel.Dispose();
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
            try
            {
                var check = new DirectoryInfo(_folderPath);
            }
            catch
            {
                return;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(_folderPath);

            DirectoryInfo[] foldersInDirectory;
            FileInfo[] filesInDirectory;

            try
            {
                foldersInDirectory = dirInfo.GetDirectories();
                filesInDirectory = dirInfo.GetFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        Size = Math.Round((file.Length / 1024.0), MidpointRounding.ToPositiveInfinity).ToString(),
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

            if (file.IsDirectory)
            {
                try
                {
                    var check = new DirectoryInfo(file.FullPath);
                }
                catch
                {
                    return;
                }

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
                        TextContent = "Loading...",
                        ImageSource = null
                    };
                    this.LazyLoadFile(true);
                });
            }
            else
            {
                _folderPath = Directory.GetCurrentDirectory();
                MessageBox.Show("Selected file doesn't exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SelectedItem = null;
            }
        }

        public async Task LazyLoadFile(bool? forceLoad = false)
        {
            if (SelectedItem == null || string.IsNullOrWhiteSpace(SelectedItem.FullPath) || !File.Exists(SelectedItem.FullPath))
                return;

            if (SelectedItem.FileContentModel.IsLoaded == true)
                return;

            var filePath = SelectedItem.FullPath;
            SelectedItem.FileContentModel = new FileContentModel();

            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            bool isImage = ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".webp" || ext == ".avif" || ext == ".gif" || ext == ".heic";
            bool isVideo = ext == ".mpg" || ext == ".mp4" || ext == ".mkv" || ext == ".webm";
            bool isAudio = ext == ".mp3" || ext == ".wav" || ext == ".aac";

            var fileInfo = new FileInfo(filePath);

            if (isImage)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        var rotatedImageBitmap = LoadImageWithOrientationHelper.LoadImageWithOrientation(filePath);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SelectedItem.FileContentModel.ImageSource = rotatedImageBitmap;
                            SelectedItem.FileContentModel.TextContent = null;
                            SelectedItem.FileContentModel.VideoMedia = null;
                            SelectedItem.FileContentModel.IsLoaded = true;
                        });
                    });
                }
                catch (Exception ex)
                {
                    SelectedItem.FileContentModel.TextContent = ex.Message;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.VideoMedia = null;
                    SelectedItem.FileContentModel.IsLoaded = false;
                }
            }
            else if (isVideo || isAudio)
            {
                try
                {
                    SelectedItem.FileContentModel.VideoMedia = new Controls.VideoPlayerControl(filePath);
                    SelectedItem.FileContentModel.TextContent = null;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.IsLoaded = true;
                }
                catch (Exception ex)
                {
                    SelectedItem.FileContentModel.TextContent = ex.Message;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.VideoMedia = null;
                    SelectedItem.FileContentModel.IsLoaded = false;
                }
            }
            else
            {
                if (fileInfo.Length < Config.MaxSizePreviewKB * 1024 || forceLoad == true)
                {
                    string asciiCharsToView = await Task.Run(() => FileContentReader.ReadTextFileAsync(filePath));
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SelectedItem.FileContentModel.TextContent = asciiCharsToView;
                        SelectedItem.FileContentModel.ImageSource = null;
                        SelectedItem.FileContentModel.VideoMedia = null;
                        SelectedItem.FileContentModel.IsLoaded = true;
                    });
                }
                else
                {
                    SelectedItem.FileContentModel.TextContent = $"File size has more than {Config.MaxSizePreviewKB} KiB, press ENTER to force load it";
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.VideoMedia = null;
                    SelectedItem.FileContentModel.IsLoaded = false;
                }
            }

            OnPropertyChanged(nameof(SelectedItem));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}