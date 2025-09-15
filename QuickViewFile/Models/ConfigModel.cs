namespace QuickViewFile.Models
{
    public class ConfigModel
    {
        public int MaxSizePreviewKB { get; set; } = 50;
        public string ImageStretch { get; set; } = "Uniform";
        public double PreviewHeight { get; set; } = 800;
        public double PreviewWidth { get; set; } = 1200;
        public double KeyboardZoomStep { get; set; } = 50;
        public string TextPreviewWordWrap { get; set; } = "NoWrap";
        public double MaxScale { get; set; } = 10.0;
        public double MinScale { get; set; } = 1;
        public double MouseWheelZoomStepFactor { get; set; } = 1.1;
        public string BitmapScalingMode { get; set; } = "HighQuality";
        public double FontSize { get; set; } = 12;
    }
}
