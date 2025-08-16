using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickViewFile.Watchers
{
    public class SelectFileWatcher
    {
        public event Action<string> OnFileSelected;
        public void SelectFile(string filePath)
        {
            // Validate the file path
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }
            // Trigger the event with the selected file path
            OnFileSelected?.Invoke(filePath);
        }
    }
}
