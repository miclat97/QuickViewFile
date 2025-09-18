using QuickViewFile.Models;
using System.Windows;
using System.Windows.Controls;

namespace QuickViewFile.Controls
{
    public class VideoPlayerWithSimpleControl : MediaElement
    {
        private readonly ConfigModel _config;

        public VideoPlayerWithSimpleControl()
        {
            _config = ConfigHelper.LoadConfig();
        }

        public bool PlayToEnd()
        {
            try
            {
                if (this.Source != null)
                {
                    this.VerticalAlignment = VerticalAlignment.Center;
                    this.HorizontalAlignment = HorizontalAlignment.Center;
                    this.Height = _config.PreviewHeight;
                    this.Width = _config.PreviewWidth;
                    this.Play();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
