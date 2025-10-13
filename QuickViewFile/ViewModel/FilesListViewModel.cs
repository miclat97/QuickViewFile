using QuickViewFile.Helpers;
using QuickViewFile.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinRT;

namespace QuickViewFile.ViewModel
{
    public class FilesListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ItemList> ActiveListItems { get; set; } = new();

        public FilesListViewModel(string folderPath)
        {
            Config = ConfigHelper.LoadConfig();

            _folderPath = Path.GetDirectoryName(folderPath)!;
            if (_folderPath.Equals(folderPath, StringComparison.OrdinalIgnoreCase)) // if directory name is the same as directory from parameter,
                                                                                    // it means that user doesn't picked any exeact file, but directory
            {
                RefreshFiles();
            }
            else
            {
                RefreshFiles(folderPath);
            }
        }

        private ItemList? _selectedItem;
        private string _folderPath;

        public Visibility TextBoxVisibility { get; set; }

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
                            SelectedItem.FileContentModel.ShowTextBox = true;
                            SelectedItem.FileContentModel.ImageSource = null;
                        }
                    }
                }
            }
        }

        public event System.Action<ItemList?>? SelectedFileChanged;

        private void RefreshFiles(string? fileToSelect = null)
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

                try
                {
                    var selectThisFile = ActiveListItems.LastOrDefault(x => x.FullPath.Equals(fileToSelect, StringComparison.OrdinalIgnoreCase));
                    if (selectThisFile is not null)
                        SelectedItem = selectThisFile;
                    else
                        SelectedItem = ActiveListItems.First();
                }
                catch
                {

                }
            });
        }

        public async Task OnFileDoubleClick(ItemList? file)
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
                //SelectedItem = null;
            }
            else if (!file.IsDirectory && File.Exists(file.FullPath))
            {
                SelectedItem = file;
                await Application.Current.Dispatcher.BeginInvoke(async () =>
                {
                    SelectedItem.FileContentModel = new FileContentModel
                    {
                        TextContent = "Loading...",
                        ShowTextBox = false,
                        VideoMedia = null,
                        ImageSource = null
                    };
                    await this.LazyLoadFile(true);
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

            var filePath = SelectedItem.FullPath;
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            bool isImage = ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".webp" || ext == ".avif" || ext == ".gif" || ext == ".heic";
            bool isVideo = ext == ".mpg" || ext == ".mp4" || ext == ".mkv" || ext == ".webm";
            bool isAudio = ext == ".mp3" || ext == ".wav" || ext == ".aac";

            var fileInfo = new FileInfo(filePath);

            // Dispose of the previous FileContentModel
            SelectedItem.FileContentModel?.Dispose();
            SelectedItem.FileContentModel = new FileContentModel();

            if (isImage)
            {
                try
                {
                    await Task.Run(() =>
                    {
                        BitmapSource? bitmap = null;
                        try
                        {
                            bitmap = LoadImageWithOrientationHelper.LoadImageWithOrientation(filePath);
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                SelectedItem.FileContentModel.TextContent = $"Error loading image: {ex.Message}";
                                SelectedItem.FileContentModel.IsLoaded = false;
                            });
                            return;
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SelectedItem.FileContentModel.ImageSource = bitmap;
                            SelectedItem.FileContentModel.TextContent = null;
                            SelectedItem.FileContentModel.IsLoaded = true;
                        });
                    });
                }
                catch (Exception ex)
                {
                    SelectedItem.FileContentModel.TextContent = ex.Message;
                    SelectedItem.FileContentModel.ShowTextBox = true;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.VideoMedia = null;
                    SelectedItem.FileContentModel.IsLoaded = false;
                }
            }
            else if (isVideo || isAudio)
            {
                try
                {
                    SelectedItem.FileContentModel.VideoMedia = new Controls.VideoPlayerControl(filePath, Config.BitmapScalingMode);
                    SelectedItem.FileContentModel.TextContent = null;
                    SelectedItem.FileContentModel.ShowTextBox = false;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.IsLoaded = true;
                }
                catch (Exception ex)
                {
                    SelectedItem.FileContentModel.TextContent = ex.Message;
                    SelectedItem.FileContentModel.ShowTextBox = true;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.IsLoaded = false;
                }
            }
            else
            {
                if (fileInfo.Length < Config.MaxSizePreviewKB * 1024 || forceLoad == true)
                {
                    //var loadedFileText = await FileContentReader.ReadTextFileAsync(filePath, Config.MaxCharsToLoad);
                    var loadedFileText = await FileContentReader.ReadTextFileAsync(filePath, Config.CharsToPreview);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SelectedItem.FileContentModel.TextContent = loadedFileText;
                        SelectedItem.FileContentModel.ImageSource = null;
                        SelectedItem.FileContentModel.IsLoaded = true;
                        SelectedItem.FileContentModel.ShowTextBox = true;
                    });
                }
                else
                {
                    SelectedItem.FileContentModel.TextContent = $"File size has more than {Config.MaxSizePreviewKB} KiB, press ENTER to force load it";
                    SelectedItem.FileContentModel.ShowTextBox = true;
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