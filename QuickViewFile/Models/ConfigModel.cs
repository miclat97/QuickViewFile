using QuickViewFile.Helpers;
using System.ComponentModel.DataAnnotations;

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
        public string ImageExtensions { get; set; } = ".jpg,.jpeg,.png,.bmp,.gif,.tiff,.ico,.webp,.avif,.heic,.jif";
        public string VideoExtensions { get; set; } = ".mp4,.avi,.mov,.wmv,.flv,.mkv,.webm,.mpg,.mpeg";
        public string MusicExtensions { get; set; } = ".mp3,.wav,.aac,.flac,.ogg,.wma,.m4a";
        [AllowedValues(new object[] { 0, 1 })]
        public int Utf8InsteadOfASCIITextPreview { get; set; } = 0;
        public int ShadowEffect { get; set; } = 1; // 0 - Disabled, 1 - Enabled
        public int ShadowQuality { get; set; } = 1; // 0 - Performance, 1 - Quality
        public int RenderMode { get; set; } = 0; // 0 - Default, 1 - SoftwareOnly
        public int EdgeMode { get; set; } = 1; // 0 - Unspecified, 1 - Aliased
        public int ThemeMode { get; set; } = 0; // 0 - System, 1 - Light, 2 - Dark
        public double ShadowDepth { get; set; } = 2;
        public double ShadowOpacity { get; set; } = 0.25;
        public double ShadowBlur { get; set; } = 25;
    }
}
