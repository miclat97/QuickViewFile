namespace QuickViewFile.Models
{
    public class ConfigModel
    {
        public int MaxSizePreviewKB { get; set; } = 50;
        public string ImageStretch { get; set; } = "Uniform";
        public double PreviewHeight { get; set; } = 800;
        public double PreviewWidth { get; set; } = 1200;
        public double ZoomStep { get; set; } = 50;
        public string TextPreviewWordWrap { get; set; } = "NoWrap";
    }
}
