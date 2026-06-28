using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using QuickViewFile.ViewModel;

namespace QuickViewFile
{
    public class SlideshowHelper
    {
        private Window _window;
        private string _mode;
        private double _slideDuration;
        private double _animDuration;
        private bool _fadeOpacity;
        private bool _fastQuality;
        private DispatcherTimer _timer;
        private Random _random = new Random();

        public SlideshowHelper(Window window, string mode, double slideDuration, double animDuration, bool fadeOpacity, bool fastQuality)
        {
            _window = window;
            _mode = mode;
            _slideDuration = slideDuration;
            _animDuration = animDuration;
            _fadeOpacity = fadeOpacity;
            _fastQuality = fastQuality;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(_slideDuration);
            _timer.Tick += Timer_Tick;
        }

        public void Start()
        {
            ResetTransforms();
            _timer.Start();
            AnimateCurrent();
        }

        public void Stop()
        {
            _timer.Stop();
            ResetTransforms();
        }

        private void ResetTransforms()
        {
            if (_window is MainWindow mw)
            {
                mw.FileContentScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, null);
                mw.FileContentScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, null);
                mw.FileContentTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);
                mw.GridFileContent.BeginAnimation(UIElement.OpacityProperty, null);

                mw.FileContentScale.ScaleX = 1;
                mw.FileContentScale.ScaleY = 1;
                mw.FileContentTranslate.X = 0;
                mw.FileContentTranslate.Y = 0;
                mw.GridFileContent.Opacity = 1;
            }
            else if (_window is MainWindowNoBorder mwnb)
            {
                mwnb.FileContentScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, null);
                mwnb.FileContentScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, null);
                mwnb.FileContentTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);
                mwnb.GridFileContent.BeginAnimation(UIElement.OpacityProperty, null);

                mwnb.FileContentScale.ScaleX = 1;
                mwnb.FileContentScale.ScaleY = 1;
                mwnb.FileContentTranslate.X = 0;
                mwnb.FileContentTranslate.Y = 0;
                mwnb.GridFileContent.Opacity = 1;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Move to next file
            if (_window.DataContext is FilesListViewModel vm)
            {
                if (vm.ActiveListItems.Count == 0) return;

                if (vm.SelectedItem == null)
                {
                    vm.SelectedItem = vm.ActiveListItems[0];
                }
                else
                {
                    int currentIndex = vm.ActiveListItems.IndexOf(vm.SelectedItem);
                    int nextIndex = currentIndex + 1;

                    // loop
                    if (nextIndex >= vm.ActiveListItems.Count)
                    {
                        nextIndex = 0;
                    }

                    vm.SelectedItem = vm.ActiveListItems[nextIndex];
                }

                if (_window is MainWindow mw)
                {
                    mw.FilesListView.ScrollIntoView(vm.SelectedItem);
                }
                else if (_window is MainWindowNoBorder mwnb)
                {
                    mwnb.FilesListView.ScrollIntoView(vm.SelectedItem);
                }

                AnimateCurrent();
            }
        }

        private void AnimateCurrent()
        {
            if (_mode == "None") return;

            string currentMode = _mode;
            if (currentMode == "Fade in/out and Random")
            {
                string[] modes = { "Slide (Right to Left)", "Center", "InOut" };
                currentMode = modes[_random.Next(modes.Length)];
            }

            UIElement? gridFileContent = null;
            System.Windows.Media.ScaleTransform? scaleTransform = null;
            System.Windows.Media.TranslateTransform? translateTransform = null;

            if (_window is MainWindow mw)
            {
                gridFileContent = mw.GridFileContent;
                scaleTransform = mw.FileContentScale;
                translateTransform = mw.FileContentTranslate;
            }
            else if (_window is MainWindowNoBorder mwnb)
            {
                gridFileContent = mwnb.GridFileContent;
                scaleTransform = mwnb.FileContentScale;
                translateTransform = mwnb.FileContentTranslate;
            }

            if (gridFileContent == null || scaleTransform == null || translateTransform == null) return;

            if (_fadeOpacity)
            {
                DoubleAnimation opacityAnim = new DoubleAnimation(0.0, 1.0, TimeSpan.FromSeconds(_animDuration));
                gridFileContent.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            }
            else
            {
                gridFileContent.Opacity = 1.0;
                gridFileContent.BeginAnimation(UIElement.OpacityProperty, null);
            }

            if (currentMode == "Center")
            {
                DoubleAnimation scaleAnim = new DoubleAnimation(0.5, 1.0, TimeSpan.FromSeconds(_animDuration));
                scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleAnim);
                scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleAnim);

                // clear translate
                translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);
                translateTransform.X = 0;
            }
            else if (currentMode == "Slide (Right to Left)")
            {
                double startX = _window.ActualWidth;
                DoubleAnimation translateAnim = new DoubleAnimation(startX, 0, TimeSpan.FromSeconds(_animDuration));

                // Ease if not fast
                if (!_fastQuality)
                {
                    translateAnim.EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };
                }

                translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, translateAnim);

                // clear scale
                scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, null);
                scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, null);
                scaleTransform.ScaleX = 1;
                scaleTransform.ScaleY = 1;
            }
            else if (currentMode == "InOut")
            {
                double startX = _window.ActualWidth;
                DoubleAnimation translateAnim = new DoubleAnimation(startX, 0, TimeSpan.FromSeconds(_animDuration));

                // Ease if not fast
                if (!_fastQuality)
                {
                    translateAnim.EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseInOut };
                }

                translateTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, translateAnim);

                // clear scale
                scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, null);
                scaleTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, null);
                scaleTransform.ScaleX = 1;
                scaleTransform.ScaleY = 1;
            }
        }
    }
}
