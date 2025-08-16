using QuickViewFile;
using QuickViewFile.Models;
using QuickViewFile.Services;
using System.Collections.ObjectModel;
using System.IO;

public class FilesListViewModel
{
    public ObservableCollection<FileItem> Files { get; set; } = new();

    private FileItem _selectedFile;
    private readonly FolderWatcher _folderWatcher;
    private string _folderPath = Directory.GetCurrentDirectory();

    public FileItem SelectedFile
    {
        get => _selectedFile;
        set
        {
            _selectedFile = value;
        }
    }

    public FilesListViewModel()
    {
        var filesInCurrentDirectory = new DirectoryInfo(_folderPath).GetFiles();

        RefreshFiles();

        // Initialize the folder watcher
        _folderWatcher = new FolderWatcher(_folderPath);

        // Subscribe to folder changes
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
}