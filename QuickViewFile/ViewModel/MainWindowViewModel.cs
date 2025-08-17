using QuickViewFile.Models;
using System;

namespace QuickViewFile.ViewModel
{
    public class MainWindowViewModel
    {
        public FilesListViewModel FilesListVM { get; }
        public FileContentViewModel FileContentVM { get; }

        public MainWindowViewModel(string folderPath)
        {
            FilesListVM = new FilesListViewModel(folderPath);
            FileContentVM = new FileContentViewModel();

            FilesListVM.SelectedFileChanged += async file =>
            {
                if (file is not null && file.IsDirectory)
                {
                    FileContentVM.FileName = file.FullPath;
                    FileContentVM.TextContent = "";
                    return;
                }
                else if (file is not null && !file.IsDirectory)
                {
                    try
                    {
                        if (file?.FullPath != null)
                            await FileContentVM.LoadTextFileAsync(file.FullPath);
                        else
                        {
                            FileContentVM.FileName = null;
                            FileContentVM.TextContent = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        FileContentVM.FileName = "B³¹d podczas otwierania pliku";
                        FileContentVM.TextContent = $"{ex.Message}";
                    }
                }
                else
                {
                    FileContentVM.FileName = null;
                    FileContentVM.TextContent = null;
                }
            };
        }
    }
}