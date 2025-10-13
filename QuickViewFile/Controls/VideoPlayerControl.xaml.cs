using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace QuickViewFile.Controls
{
    /// <summary>
    /// Interaction logic for VideoPlayerControl .xaml
    /// </summary>
    public partial class VideoPlayerControl : UserControl, IDisposable
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private bool isVideoPaused = false;
        private bool disposedValue;

        private readonly string videoQuality;

        //private readonly ConfigModel _config;

        public VideoPlayerControl()
        {
            InitializeComponent();

            //_config = ConfigHelper.LoadConfig();
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += timer_Tick;
            timer.Start();
        }

        public VideoPlayerControl(string filePath, string qualityFromConfig)
        {
            videoQuality = qualityFromConfig;
            InitializeComponent();

            //_config = ConfigHelper.LoadConfig();
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += timer_Tick;
            timer.Start();

            StartPlaying(filePath);
        }

        public void StartPlaying(string filePath)
        {
            videoInWindowPlayer.Source = new Uri(filePath);
            videoInWindowPlayer.Play();
            isVideoPaused = false;
            mediaPlayerIsPlaying = true;
            videoInWindowPlayer.Volume = 1;
            //videoInWindowPlayer.Height = _config.VideoHeigth;
            //videoInWindowPlayer.RenderSize = new Size(_config.VideoWidth, _config.VideoHeigth);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((videoInWindowPlayer.Source != null) && (videoInWindowPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = videoInWindowPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = videoInWindowPlayer.Position.TotalSeconds;
            }
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            PlayButton.Opacity = 1;
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                videoInWindowPlayer.Stop();
            }
            videoInWindowPlayer.Play();
            isVideoPaused = false;
            mediaPlayerIsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying && !isVideoPaused)
            {
                e.CanExecute = true;
                PauseButton.Opacity = 1;
            }
            else
            {
                e.CanExecute = false;
                PauseButton.Opacity = 0.1;
            }
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            videoInWindowPlayer.Pause();
            isVideoPaused = true;
            mediaPlayerIsPlaying = false;
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            StopButton.Opacity = 1;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            videoInWindowPlayer.Stop();
            isVideoPaused = true;
            mediaPlayerIsPlaying = false;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            videoInWindowPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            videoInWindowPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
            fullTime.Text = TimeSpan.FromSeconds(sliProgress.Maximum).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            videoInWindowPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void PlayOrPauseMedia()
        {
            if (!isVideoPaused)
            {
                videoInWindowPlayer.Pause();
                isVideoPaused = true;
                mediaPlayerIsPlaying = false;
            }
            else
            {
                videoInWindowPlayer.Play();
                isVideoPaused = false;
                mediaPlayerIsPlaying = true;
            }
        }

        private void videoPlayer_KeyDown(object sender, KeyEventArgs e)
        {
            //HandleKey(e.Key);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.PreviewKeyDown += ParentWindow_PreviewKeyDown;
            }
        }

        private void ParentWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            HandleKey(e.Key);
        }

        public void HandleKey(Key key)
        {
            if (key == Key.Space)
            {
                PlayOrPauseMedia();
            }
            else if (key == Key.Add)
            {
                videoInWindowPlayer.Volume += 0.1;
            }
            else if (key == Key.Subtract)
            {
                videoInWindowPlayer.Volume -= 0.1;
            }
            else if (key == Key.Left)
            {
                TimeSpan actualTime = videoInWindowPlayer.Position;
                TimeSpan newTime = actualTime.Subtract(TimeSpan.FromSeconds(10));
                videoInWindowPlayer.Position = newTime;
            }
            else if (key == Key.Right)
            {
                TimeSpan actualTime = videoInWindowPlayer.Position;
                TimeSpan newTime = actualTime.Add(TimeSpan.FromSeconds(10));
                videoInWindowPlayer.Position = newTime;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    videoInWindowPlayer.Stop();
                    mediaPlayerIsPlaying = false;
                    videoInWindowPlayer.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public TimeSpan GetCurrentVideoPosition()
        {
            return TimeSpan.FromSeconds(sliProgress.Value);
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~videoInWindowPlayerControl()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
