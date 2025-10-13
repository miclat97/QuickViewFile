using System.Windows;
using System.Windows.Input;

namespace QuickViewFile.Controls
{
    /// <summary>
    /// Interaction logic for VideoPlayerFullScreen.xaml
    /// </summary>
    public partial class VideoPlayerFullScreen : Window
    {
        private bool mediaPlayerIsPlaying = false;
        private readonly bool userIsDraggingSlider = false;
        private bool isVideoPaused = false;
        private bool disposedValue;

        private readonly string videoQuality;

        //private readonly ConfigModel _config;

        public VideoPlayerFullScreen()
        {
            InitializeComponent();

            //_config = ConfigHelper.LoadConfig();
        }

        public VideoPlayerFullScreen(string filePath, string videoQualityFromConfig, TimeSpan position)
        {
            videoQuality = videoQualityFromConfig;
            InitializeComponent();
            //videoFullScreenPlayer.Height = heigth;
            //_config = ConfigHelper.LoadConfig();
            StartPlaying(filePath, position);
        }

        public void StartPlaying(string filePath, TimeSpan positionFromContstructor)
        {
            videoFullScreenPlayer.Source = new Uri(filePath);
            videoFullScreenPlayer.Play();
            videoFullScreenPlayer.Position = positionFromContstructor;

            isVideoPaused = false;
            mediaPlayerIsPlaying = true;
            videoFullScreenPlayer.Volume = 1;
            //videoFullScreenPlayer.Height = _config.VideoHeigth;
            //videoFullScreenPlayer.RenderSize = new Size(_config.VideoWidth, _config.VideoHeigth);
        }


        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                videoFullScreenPlayer.Stop();
            }
            videoFullScreenPlayer.Play();
            isVideoPaused = false;
            mediaPlayerIsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (mediaPlayerIsPlaying && !isVideoPaused)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            videoFullScreenPlayer.Pause();
            isVideoPaused = true;
            mediaPlayerIsPlaying = false;
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            videoFullScreenPlayer.Stop();
            isVideoPaused = true;
            mediaPlayerIsPlaying = false;
        }


        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            videoFullScreenPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void PlayOrPauseMedia()
        {
            if (!isVideoPaused)
            {
                videoFullScreenPlayer.Pause();
                isVideoPaused = true;
                mediaPlayerIsPlaying = false;
            }
            else
            {
                videoFullScreenPlayer.Play();
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
                videoFullScreenPlayer.Volume += 0.1;
            }
            else if (key == Key.Subtract)
            {
                videoFullScreenPlayer.Volume -= 0.1;
            }
            else if (key == Key.Left)
            {
                TimeSpan actualTime = videoFullScreenPlayer.Position;
                TimeSpan newTime = actualTime.Subtract(TimeSpan.FromSeconds(10));
                videoFullScreenPlayer.Position = newTime;
            }
            else if (key == Key.Right)
            {
                TimeSpan actualTime = videoFullScreenPlayer.Position;
                TimeSpan newTime = actualTime.Add(TimeSpan.FromSeconds(10));
                videoFullScreenPlayer.Position = newTime;
            }
        }

        public TimeSpan GetCurrentVideoPosition()
        {
            return userIsDraggingSlider ? videoFullScreenPlayer.Position : TimeSpan.Zero;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    videoFullScreenPlayer.Stop();
                    mediaPlayerIsPlaying = false;
                    videoFullScreenPlayer.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~videoFullScreenPlayerControl()
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Dispose();
                Close();
            }
        }
    }
}
