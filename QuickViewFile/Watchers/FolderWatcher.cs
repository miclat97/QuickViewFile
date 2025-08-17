using System.IO;

namespace QuickViewFile.Watchers
{
    public class FolderWatcher : IDisposable
    {
        private FileSystemWatcher _watcher;

        public event Action OnFolderChanged;

        public FolderWatcher(string folderPath)
        {
            _watcher = new FileSystemWatcher(folderPath)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnChanged;
            _watcher.Deleted += OnChanged;
            _watcher.Changed += OnChanged;
            _watcher.Renamed += OnChanged;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            OnFolderChanged?.Invoke();
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
