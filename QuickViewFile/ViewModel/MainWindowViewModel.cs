using QuickViewFile.Models;
using System;

namespace QuickViewFile.ViewModel
{
    public class MainWindowViewModel
    {
        public FilesListViewModel FilesListVM { get; }
        //public FileContentViewModel FileContentVM { get; }

        public MainWindowViewModel(string folderPath)
        {
            FilesListVM = new FilesListViewModel(folderPath);

            FilesListVM.SelectedFileChanged += async file =>
            {
                if (file is not null && !file.IsDirectory)
                {
                    try
                    {
                        if (file?.FullPath != null)
                            await FilesListVM.LazyLoadFileAsync(false);
                    }
                    catch (Exception ex)
                    {
                        file.FileContentModel.TextContent = $"{ex.Message}";
                        file.FileContentModel.ImageSource = null;
                    }
                }
            };
        }
    }
}