using QuickViewFile.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace QuickViewFile.Controls
{
    /// <summary>
    /// Interaction logic for VideoPlayerControl.xaml
    /// </summary>
    public partial class VideoPlayerControl : UserControl, IDisposable
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private bool isVideoPaused = false;
        private bool disposedValue;

        //private readonly ConfigModel _config;

        public VideoPlayerControl()
        {
            InitializeComponent();

            //_config = ConfigHelper.LoadConfig();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        public VideoPlayerControl(string filePath)
        {
            InitializeComponent();

            //_config = ConfigHelper.LoadConfig();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            StartPlaying(filePath);
        }

        public void StartPlaying(string filePath)
        {
            videoPlayer.Source = new Uri(filePath);
            videoPlayer.Play();
            isVideoPaused = false;
            mediaPlayerIsPlaying = true;
            videoPlayer.Volume = 1;
            //videoPlayer.Height = _config.VideoHeigth;
            //videoPlayer.RenderSize = new Size(_config.VideoWidth, _config.VideoHeigth);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((videoPlayer.Source != null) && (videoPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = videoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = videoPlayer.Position.TotalSeconds;
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
                videoPlayer.Stop();
            }
            videoPlayer.Play();
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
            videoPlayer.Pause();
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
            videoPlayer.Stop();
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
            videoPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            videoPlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
            fullTime.Text = TimeSpan.FromSeconds(sliProgress.Maximum).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            videoPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void PlayOrPauseMedia()
        {
            if (!isVideoPaused)
            {
                videoPlayer.Pause();
                isVideoPaused = true;
                mediaPlayerIsPlaying = false;
            }
            else
            {
                videoPlayer.Play();
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
                videoPlayer.Volume += 0.1;
            }
            else if (key == Key.Subtract)
            {
                videoPlayer.Volume -= 0.1;
            }
            else if (key == Key.Left)
            {
                TimeSpan actualTime = videoPlayer.Position;
                TimeSpan newTime = actualTime.Subtract(TimeSpan.FromSeconds(10));
                videoPlayer.Position = newTime;
            }
            else if (key == Key.Right)
            {
                TimeSpan actualTime = videoPlayer.Position;
                TimeSpan newTime = actualTime.Add(TimeSpan.FromSeconds(10));
                videoPlayer.Position = newTime;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    videoPlayer.Stop();
                    mediaPlayerIsPlaying = false;
                    videoPlayer.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~VideoPlayerControl()
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
