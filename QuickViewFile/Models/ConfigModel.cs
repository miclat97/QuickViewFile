namespace QuickViewFile.Models
{
    public class ConfigModel
    {
        public int MaxSizePreviewKB { get; set; } = 50;
        public string ImageStretch { get; set; } = "Uniform";
        public double PreviewHeight { get; set; } = 800;
        public double PreviewWidth { get; set; } = 1200;
        public double VideoHeigth { get; set; } = 1080;
        public double VideoWidth { get; set; } = 1920;
        public double KeyboardZoomStep { get; set; } = 50;
        public string TextPreviewWordWrap { get; set; } = "Wrap";
        public double MaxScale { get; set; } = 100.0;
        public double MinScale { get; set; } = 1;
        public double MouseWheelZoomStepFactor { get; set; } = 1.2;
        public string BitmapScalingMode { get; set; } = "Fant";
        public double FontSize { get; set; } = 13;
        public double CharsToPreview { get; set; } = 100000000;
    }
}
