using QuickViewFile.Models;
using QuickViewFile.Watchers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace QuickViewFile.ViewModel
{
    public class FilesListViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FileItem> Files { get; set; } = new();

        private FileItem? _selectedFile;
        private readonly FolderWatcher _folderWatcher;
        private string _folderPath = Directory.GetCurrentDirectory();

        public FileItem? SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    OnPropertyChanged(nameof(SelectedFile));
                    SelectedFileChanged?.Invoke(_selectedFile);
                }
            }
        }

        public event System.Action<FileItem?>? SelectedFileChanged;

        public FilesListViewModel()
        {
            RefreshFiles();
            _folderWatcher = new FolderWatcher(_folderPath);
            _folderWatcher.OnFolderChanged += RefreshFiles;
        }

        private async void RefreshFiles()
        {
            var filesInDirectory = await Task.Run(() => new DirectoryInfo(_folderPath).GetFiles());
            App.Current.Dispatcher.Invoke(() =>
            {
                Files.Clear();
                foreach (var file in filesInDirectory)
                {
                    Files.Add(new FileItem
                    {
                        Name = file.Name,
                        Size = file.Length,
                        FullPath = file.FullName
                    });
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}