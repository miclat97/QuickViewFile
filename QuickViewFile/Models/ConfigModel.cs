using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace QuickViewFile.Models
{
    public class ConfigModel
    {
        public int MaxSizePreviewKB { get; set; } = 50;
        public string ImageStretch { get; set; } = "Uniform";
        public int PreviewHeight { get; set; } = 800;
        public int PreviewWidth { get; set; } = 1200;
        public string TextPreviewWordWrap { get; set; } = "NoWrap";
    }
}
