using QuickViewFile.Models;
using QuickViewFile.Watchers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

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
                // Read directories and files synchronously
                (foldersInDirectory, filesInDirectory) = await Task.Run(() =>
                {
                    var folders = dirInfo.GetDirectories();
                    var files = dirInfo.GetFiles();
                    return (folders, files);
                });
            }
            catch
            {
                // If unable to read the directory, clear the list and exit
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
                        IsDirectory = true
                    });
                }

                foreach (var folder in foldersInDirectory)
                {
                    ActiveListItems.Add(new ItemList
                    {
                        Name = folder.Name,
                        Size = "",
                        FullPath = folder.FullName,
                        IsDirectory = true
                    });
                }

                foreach (var file in filesInDirectory)
                {
                    ActiveListItems.Add(new ItemList
                    {
                        Name = file.Name,
                        Size = file.Length.ToString(),
                        FullPath = file.FullName,
                        IsDirectory = false
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
                _folderPath = file.FullPath!;
            }

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}