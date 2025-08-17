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
                if (file is not null && !file.IsDirectory)
                {
                    try
                    {
                        if (file?.FullPath != null)
                            await FileContentVM.LoadFileAsync(file.FullPath);
                        else
                        {
                            FileContentVM.FileName = null;
                            FileContentVM.TextContent = null;
                            FileContentVM.ImageSource = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        FileContentVM.FileName = "B³¹d podczas otwierania pliku";
                        FileContentVM.TextContent = $"{ex.Message}";
                        FileContentVM.ImageSource = null;
                    }
                }
                else
                {
                    FileContentVM.FileName = null;
                    FileContentVM.TextContent = null;
                    FileContentVM.ImageSource = null;
                }
            };
        }
    }
}