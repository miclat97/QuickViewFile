using QuickViewFile.Helpers;

namespace QuickViewFile.Models
{
    public class ConfigParsedModel : ConfigModel
    {
        public List<string> ImageExtensionsParsed { get; set; } = new();
        public List<string> VideoExtensionsParsed { get; set; } = new();
        public List<string> MusicExtensionsParsed { get; set; } = new();
    }
}