using QuickViewFile.Helpers;
using QuickViewFile.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace QuickViewFile.ViewModel
{
    public class FilesListViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ItemList> _activeListItems = [];
        public ObservableCollection<ItemList> ActiveListItems
        {
            get => _activeListItems;
            set
            {
                _activeListItems = value;
                OnPropertyChanged(nameof(ActiveListItems));
            }
        }

        public FilesListViewModel(string folderPath)
        {
            Config = ConfigHelper.loadedConfig;

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
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            StatusBarText = $"QuickViewFile v {appVersion}";
        }

        private ItemList? _selectedItem;
        private string _folderPath;
        public string FolderPath => _folderPath;

        public Visibility TextBoxVisibility { get; set; }

        public ConfigModel Config { get; set; } = ConfigHelper.loadedConfig;

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

        public string StatusBarText { get; set; }

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

        public void RefreshFiles(string? fileToSelect = null)
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
                foreach (var item in ActiveListItems)
                {
                    item.ThumbnailImageSource = null;
                    item.IsVideoThumbnail = false;
                    item.ThumbnailTextPreview = null;
                }

                ActiveListItems.Clear();

                if (dirInfo.Parent != null)
                {
                    var parentDir = new ItemList
                    {
                        Name = "..",
                        Size = "",
                        FullPath = dirInfo.Parent.FullName,
                        IsDirectory = true,
                        FileContentModel = new FileContentModel()
                    };

                    parentDir.PropertyChanged += ParentDir_PropertyChanged;

                    ActiveListItems.Add(parentDir);
                    if(fileToSelect == null)
                    {
                        SelectedItem = parentDir;
                    }
                }

                foreach (DirectoryInfo folder in foldersInDirectory)
                {
                    ActiveListItems.Add(new ItemList
                    {
                        Name = folder.Name,
                        Size = "",
                        FullPath = folder.FullName,
                        IsDirectory = true,
                        LastModified = folder.LastWriteTime,
                        LastModifiedString = folder.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                        FileContentModel = new FileContentModel()
                    });
                }

                foreach (FileInfo file in filesInDirectory)
                {
                    ActiveListItems.Add(new ItemList
                    {
                        Name = file.Name,
                        Size = Math.Round((file.Length / 1024.0), MidpointRounding.ToPositiveInfinity).ToString(),
                        SizeBytes = file.Length,
                        FullPath = file.FullName,
                        IsDirectory = false,
                        LastModified = file.LastWriteTime,
                        LastModifiedString = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                        FileContentModel = new FileContentModel()
                    });

                    if (Config.ShowAlternateDataStreams == 1)
                    {
                        var streams = AdsHelper.GetAlternateDataStreams(file.FullName);
                        foreach (var stream in streams)
                        {
                            ActiveListItems.Add(new ItemList
                            {
                                Name = $"{file.Name}:{stream.Name}",
                                Size = Math.Round((stream.Size / 1024.0), MidpointRounding.ToPositiveInfinity).ToString(),
                                SizeBytes = stream.Size,
                                FullPath = $"{file.FullName}:{stream.Name}",
                                IsDirectory = false,
                                LastModified = file.LastWriteTime,
                                LastModifiedString = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                                FileContentModel = new FileContentModel(),
                                IsAlternativeDataStream = true
                            });
                        }
                    }
                }
                if (fileToSelect == null)
                {
                    SelectedItem = ActiveListItems.FirstOrDefault();
                }
                else
                {
                    try
                    {
                        ItemList? selectThisFile = null;
                        if (!string.IsNullOrWhiteSpace(fileToSelect))
                        {
                            string targetName = Path.GetFileName(fileToSelect);
                            selectThisFile = ActiveListItems.FirstOrDefault(x => string.Equals(x.Name, targetName, StringComparison.OrdinalIgnoreCase) && !x.IsDirectory);
                        }
                        if (selectThisFile is not null)
                            SelectedItem = selectThisFile;
                        else
                            SelectedItem = ActiveListItems.FirstOrDefault();
                    }
                    catch
                    {

                    }
                }
                if (IsThumbnailMode)
                {
                    _ = LoadThumbnailsAsync();
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
                await Application.Current.Dispatcher.BeginInvoke(async () =>
                {
                    if (SelectedItem != null)
                    {
                        SelectedItem.FileContentModel = new FileContentModel
                        {
                            TextContent = "Loading...",
                            ShowTextBox = false,
                            VideoMedia = null,
                            ImageSource = null
                        };
                    }
                    await LazyLoadFile(true);
                });
            }
            else
            {
                _folderPath = Directory.GetCurrentDirectory();
                MessageBox.Show("Selected file doesn't exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LazyLoadFile(bool? forceLoad = false)
        {
            if (SelectedItem == null || string.IsNullOrWhiteSpace(SelectedItem.FullPath) || !File.Exists(SelectedItem.FullPath))
                return;

            string filePath = SelectedItem.FullPath;
            string extensionPath = filePath;

            string fileName = Path.GetFileName(filePath);
            bool isAds = false;
            if (fileName != null && fileName.Contains(':'))
            {
                int colonIndex = extensionPath.LastIndexOf(':');
                if (colonIndex > 3) // More than drive letter like C:\
                {
                    extensionPath = extensionPath.Substring(0, colonIndex);
                    isAds = true;
                }
            }

            string ext = Path.GetExtension(extensionPath).ToLowerInvariant();

            FileTypeEnum fileType = FileTypeEnum.Text;
            var imageExtensions = ConfigHelper.GetStringsFromCommaSeparatedString(Config.ImageExtensions);
            var musicExtensions = ConfigHelper.GetStringsFromCommaSeparatedString(Config.MusicExtensions);
            var videoExtension = ConfigHelper.GetStringsFromCommaSeparatedString(Config.VideoExtensions);
            var liveStreamExtensions = ConfigHelper.GetStringsFromCommaSeparatedString(Config.LiveStreamExtensions);
            if (ext != null && !isAds) // Default to Text for ADS because many video/image formats cannot be streamed directly from ADS
            {
                if (liveStreamExtensions.Contains(ext))
                {
                    fileType = FileTypeEnum.LiveStream;
                }
                if (imageExtensions.Contains(ext))
                {
                    fileType = FileTypeEnum.Image;
                }
                if (musicExtensions.Contains(ext) || videoExtension.Contains(ext))
                {
                    fileType = FileTypeEnum.Multimedia;
                }
            }

            FileInfo fileInfo = new FileInfo(filePath);

            // Dispose of the previous FileContentModel
            SelectedItem.FileContentModel?.Dispose();
            SelectedItem.FileContentModel = new FileContentModel();

            if (fileType == FileTypeEnum.LiveStream)
            {
                try
                {
                    string streamUrl = FileContentReader.ExtractStreamUrlFromM3u(filePath);
                    SelectedItem.FileContentModel.VideoMedia = new Controls.VideoPlayerControl(streamUrl, Config.BitmapScalingMode);
                    SelectedItem.FileContentModel.TextContent = null;
                    SelectedItem.FileContentModel.ShowTextBox = false;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.IsLoaded = true;
                }
                catch (Exception ex)
                {
                    SelectedItem.FileContentModel.TextContent = ex.Message;
                    SelectedItem.FileContentModel.VideoMedia = null;
                    SelectedItem.FileContentModel.ShowTextBox = true;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.IsLoaded = false;
                }
            }
            else if (fileType == FileTypeEnum.Image)
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
                    SelectedItem.FileContentModel.VideoMedia = null;
                    SelectedItem.FileContentModel.ShowTextBox = true;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.IsLoaded = false;
                }
            }
            else
            {
                try
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
                catch (Exception ex)
                {

                    SelectedItem.FileContentModel.TextContent = ex.Message;
                    SelectedItem.FileContentModel.VideoMedia = null;
                    SelectedItem.FileContentModel.ShowTextBox = true;
                    SelectedItem.FileContentModel.ImageSource = null;
                    SelectedItem.FileContentModel.IsLoaded = false;
                }
            }
            OnPropertyChanged(nameof(SelectedItem));
        }

        private async Task<string> _ReadTextFileAsync(string filePath, double maxChars)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            Encoding encoding;
            if (Config.Utf8InsteadOfASCIITextPreview == 1)
            {
                encoding = Encoding.GetEncoding(
                    "utf-8",
                    new EncoderReplacementFallback(""),
                    new DecoderReplacementFallback("")
                );
            }
            else // Latin1
            {
                encoding = Encoding.GetEncoding(
                    "iso-8859-1",
                    new EncoderReplacementFallback(""),
                    new DecoderReplacementFallback("")
                );
            }

            using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            using StreamReader reader = new StreamReader(fileStream, encoding);

            // When we use ArrayPool, GC will not be invokated, so we can do it much faster
            char[] buffer = System.Buffers.ArrayPool<char>.Shared.Rent((int)maxChars);

            try
            {
                int totalCharsRead = 0;
                int charsRead;

                // asyncrhonious read to buffer chars
                while (totalCharsRead < maxChars &&
                       (charsRead = await reader.ReadAsync(buffer, totalCharsRead, (int)maxChars - totalCharsRead)) > 0)
                {
                    totalCharsRead += charsRead;
                }

                bool isTruncated = (totalCharsRead == maxChars);

                // count how mant valid chars is to read (to alocate memory efficienty)
                int validCount = 0;
                for (int i = 0; i < totalCharsRead; i++)
                {
                    if (FileTextExtractor.IsPrintable(buffer[i])) validCount++;
                }

                // build text from buffer with only valid chars
                string result = string.Create(validCount, (buffer, totalCharsRead), (span, state) =>
                {
                    int index = 0;
                    for (int i = 0; i < state.totalCharsRead; i++)
                    {
                        char c = state.buffer[i];
                        if (FileTextExtractor.IsPrintable(c))
                        {
                            span[index++] = c;
                        }
                    }
                });

                if (isTruncated)
                {
                    result += "\n\n[File truncated - too large to display completely]";
                }

                return result;
            }
            finally
            {
                // Zwracamy bufor do puli pamięci, dzięki czemu kolejne podglądy plików użyją tej samej pamięci
                System.Buffers.ArrayPool<char>.Shared.Return(buffer);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool _isThumbnailMode = false;
        public bool IsThumbnailMode
        {
            get => _isThumbnailMode;
            set
            {
                _isThumbnailMode = value;
                OnPropertyChanged(nameof(IsThumbnailMode));
            }
        }

        private double _thumbnailSize = 200;
        public double ThumbnailSize
        {
            get => _thumbnailSize;
            set
            {
                _thumbnailSize = value;
                OnPropertyChanged(nameof(ThumbnailSize));
            }
        }

        private double _thumbnailFontSize = 13;
        public double ThumbnailFontSize
        {
            get => _thumbnailFontSize;
            set
            {
                _thumbnailFontSize = value;
                OnPropertyChanged(nameof(ThumbnailFontSize));
            }
        }

        private System.Threading.CancellationTokenSource? _thumbnailCancellationTokenSource;

        public void CancelThumbnails()
        {
            _thumbnailCancellationTokenSource?.Cancel();
        }

        public async System.Threading.Tasks.Task LoadThumbnailsAsync()
        {
            _thumbnailCancellationTokenSource?.Cancel();
            _thumbnailCancellationTokenSource = new System.Threading.CancellationTokenSource();
            var token = _thumbnailCancellationTokenSource.Token;

            var config = ConfigHelper.loadedConfig;
            var itemsToLoad = ActiveListItems.ToList();

            // Limit concurrency based on CPU cores
            int maxConcurrency = Math.Max(1, Environment.ProcessorCount / 4);
            using var semaphore = new System.Threading.SemaphoreSlim(maxConcurrency);

            var tasks = itemsToLoad.Select(async item =>
            {
                if (token.IsCancellationRequested) return;

                bool lockAcquired = false;
                try
                {
                    await semaphore.WaitAsync(token);
                    lockAcquired = true;

                    if (!token.IsCancellationRequested)
                    {
                        await item.LoadThumbnailAsync(config, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation
                }
                finally
                {
                    if (lockAcquired)
                    {
                        semaphore.Release();
                    }
                }
            });

            try
            {
                await System.Threading.Tasks.Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void ParentDir_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 1. Sprawdzamy, czy powiadomienie dotyczy konkretnie właściwości "IsChecked" 
            // (żeby nie reagować np. na zmianę nazwy)
            if (e.PropertyName == nameof(ItemList.IsChecked) && sender is ItemList parentDir)
            {
                // 2. Jeśli IsChecked się zmieniło, przelatujemy przez wszystkie pliki na liście
                foreach (var item in ActiveListItems)
                {
                    // 3. Omijamy foldery oraz sam element ".."
                    if (item.Name != "..")
                    {
                        // 4. Ustawiamy status pliku na dokładnie taki sam, jaki ma element ".."
                        item.IsChecked = parentDir.IsChecked;
                    }
                }
            }
        }
    }
}