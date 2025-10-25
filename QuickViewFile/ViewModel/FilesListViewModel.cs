using QuickViewFile.Helpers;
using QuickViewFile.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace QuickViewFile.ViewModel
{
    public class FilesListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ItemList> ActiveListItems { get; set; } = [];

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
                            LazyLoadFile(false);
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
                DirectoryInfo check = new DirectoryInfo(_folderPath);
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

                foreach (DirectoryInfo folder in foldersInDirectory)
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

                foreach (FileInfo file in filesInDirectory)
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
                    ItemList? selectThisFile = ActiveListItems.LastOrDefault(x => x.FullPath.Equals(fileToSelect, StringComparison.OrdinalIgnoreCase));
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
                    DirectoryInfo check = new DirectoryInfo(file.FullPath);
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
                    await LazyLoadFile(true);
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

            string filePath = SelectedItem.FullPath;
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            FileTypeEnum fileType = FileTypeEnum.Text;

            if (ext != null)
            {
                if (ConfigHelper.GetStringsFromCommaSeparatedString(Config.ImageExtensions).Contains(ext))
                {
                    fileType = FileTypeEnum.Image;
                }
                if (ConfigHelper.GetStringsFromCommaSeparatedString(Config.VideoExtensions).Contains(ext) ||
                    ConfigHelper.GetStringsFromCommaSeparatedString(Config.MusicExtensions).Contains(ext))
                {
                    fileType = FileTypeEnum.Multimedia;
                }
            }

            FileInfo fileInfo = new FileInfo(filePath);

            // Dispose of the previous FileContentModel
            SelectedItem.FileContentModel?.Dispose();
            SelectedItem.FileContentModel = new FileContentModel();

            if (fileType == FileTypeEnum.Image)
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
            else if (fileType == FileTypeEnum.Multimedia)
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
                    string loadedFileText = await _ReadTextFileAsync(filePath, Config.CharsToPreview);
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

        private async Task<string> _ReadTextFileAsync(string filePath, double maxChars)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true); // Async file stream
                {
                    using StreamReader reader = new StreamReader(fileStream, Encoding.UTF8, true, 4096, true); // Add encoding and buffer size
                    StringBuilder result = new StringBuilder();
                    char[] buffer = new char[4096];
                    int charsRead;

                    while ((charsRead = await reader.ReadAsync(buffer, 0, Math.Min(buffer.Length, (int)maxChars - result.Length))) > 0 && result.Length < maxChars)
                    {
                        string bufferContent;
                        if (Config.Utf8InsteadOfASCIITextPreview == 1)
                        {
                            bufferContent = new string(buffer, 0, charsRead).ToUtf8();
                        }
                        else if (Config.Utf8InsteadOfASCIITextPreview == 0)
                        {
                            bufferContent = new string(buffer, 0, charsRead).ToAscii();
                        }
                        result.Append(buffer);
                    }

                    if (result.Length == maxChars)
                    {
                        result.AppendLine("\n[File truncated - too large to display completely]");
                    }

                    return result.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}