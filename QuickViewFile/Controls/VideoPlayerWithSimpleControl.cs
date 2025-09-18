using QuickViewFile.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuickViewFile.Controls
{
    public class VideoPlayerWithSimpleControl : MediaElement
    {
        private readonly ConfigModel _config;
        private TimeSpan actualPosition = TimeSpan.Zero;

        public VideoPlayerWithSimpleControl()
        {
            _config = ConfigHelper.LoadConfig();
        }

        public VideoPlayerWithSimpleControl(string? pathToFile = null)
        {
            _config = ConfigHelper.LoadConfig();
        }

        public bool StartPlaying(string pathToFile)
        {
            actualPosition = TimeSpan.Zero;

            this.Source = new Uri(pathToFile);
            this.Stretch = Stretch.Uniform;
            this.StretchDirection = StretchDirection.Both;
            this.LoadedBehavior = MediaState.Manual;
            this.Visibility = Visibility.Visible;
            this.Volume = 0.5;
            this.Position = actualPosition;
            this.Play();
            return true;
        }

        public void StopPlaying()
        {
            this.Stop();
            this.Source = null;
            this.Visibility = Visibility.Hidden;
        }

        public void RestartPlaying()
        {
            if (this.Source is not null)
            {
                var filePath = this.Source.ToString();
                this.StopPlaying();
                this.StartPlaying(filePath);
            }
        }

        public void PausePlaying()
        {
            this.Pause();
            actualPosition = this.Position;
        }

        public void ResumePlaying()
        {
            this.Play();
        }

        public void Rewind(TimeSpan duration)
        {
            this.Position += duration;
        }

        public void VolumeUp(int percentVolumeToUp)
        {
            if (percentVolumeToUp <= 0)
                this.Volume = 0;
            else if (percentVolumeToUp >= 100)
                this.Volume = 1;
            else
            {
                double volumePercentageChange = (double)percentVolumeToUp / 100;
                double newVolumeToSet = this.Volume + volumePercentageChange;
                if (newVolumeToSet <= 0)
                    this.Volume = 0;
                else if (newVolumeToSet >= 100)
                    this.Volume = 1;
                else
                    this.Volume = newVolumeToSet;
            }
        }

        public void VolumeDown(int percentVolumeToDown)
        {
            if (percentVolumeToDown >= 100)
                this.Volume = 1;
            else if (percentVolumeToDown <= 100)
                this.Volume = 0;
            else
            {
                double volumePercentageChange = (double)percentVolumeToDown / 100;
                double newVolumeToSet = this.Volume - volumePercentageChange;
                if (newVolumeToSet <= 0)
                    this.Volume = 0;
                else if (newVolumeToSet >= 100)
                    this.Volume = 1;
                else
                    this.Volume = newVolumeToSet;
            }
        }
    }
}
