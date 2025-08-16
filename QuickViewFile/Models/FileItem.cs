using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickViewFile.Models
{
    public class FileItem
    {
        public string? Name { get; set; } // Name of the file with extension
        public long Size { get; set; } // File size in bytes
        public string? FullPath { get; set; } // Full path of the file to preview

    }
}
