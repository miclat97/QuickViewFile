using QuickViewFile.Models;
using System.Collections.ObjectModel;
using System.IO;

public class FilesListViewModel
{
    public ObservableCollection<FileItem> Files { get; set; } = new();

    private FileItem _selectedFile;
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
        var currentDir = Directory.GetCurrentDirectory();
        var filesInCurrentDirectory = new DirectoryInfo(currentDir).GetFiles();

        foreach (var file in filesInCurrentDirectory)
        {
            Files.Add(new FileItem
            {
                Name = file.Name,
                Size = file.Length,
                FullPath = file.FullName
            });
        }
    }
}