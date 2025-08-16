using QuickViewFile.Models;

namespace QuickViewFile.ViewModel
{
    public class MainWindowViewModel
    {
        public FilesListViewModel FilesListVM { get; }
        public FileContentViewModel FileContentVM { get; }

        public MainWindowViewModel()
        {
            FilesListVM = new FilesListViewModel();
            FileContentVM = new FileContentViewModel();

            FilesListVM.SelectedFileChanged += async file =>
            {
                if (file?.FullPath != null)
                    await FileContentVM.LoadTextFileAsync(file.FullPath, file.Size);
                else
                {
                    FileContentVM.FileName = null;
                    FileContentVM.TextContent = null;
                }
            };
        }
    }
}