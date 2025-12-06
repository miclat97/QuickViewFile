using QuickViewFile.Helpers;
using QuickViewFile.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuickViewFile.Controls
{
    public class ZoomableImage : Image
    {
        private readonly ConfigModel _config;
        private Point? lastDragPoint;
        private double currentScale = 1.0;
        private readonly TranslateTransform translateTransform = new TranslateTransform();
        private readonly ScaleTransform scaleTransform = new ScaleTransform();
        private readonly TransformGroup transformGroup = new TransformGroup();

        // 1. Monitoring source property changes
        static ZoomableImage()
        {
            SourceProperty.OverrideMetadata(typeof(ZoomableImage), new FrameworkPropertyMetadata(OnSourceChanged));
        }

        // 2. Invoked when the image changes: Resets the state
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ZoomableImage)?.ResetTransforms();
        }

        // 3. Reset only transforms. Centering is handled by XAML.
        private void ResetTransforms()
        {
            currentScale = 1.0;
            scaleTransform.ScaleX = 1.0;
            scaleTransform.ScaleY = 1.0;
            translateTransform.X = 0.0;
            translateTransform.Y = 0.0;
        }

        public ZoomableImage()
        {
            _config = ConfigHelper.loadedConfig;

            if (_config?.ShadowEffect == 1)
            {
                System.Windows.Media.Effects.DropShadowEffect dropShadow = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Gray,
                    RenderingBias = _config.ShadowQuality == 1 ? System.Windows.Media.Effects.RenderingBias.Quality : System.Windows.Media.Effects.RenderingBias.Performance,
                };
                Effect = dropShadow;
            }


            UseLayoutRounding = true;
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);

            //ClipToBounds = true;

            RenderTransform = transformGroup;

            // Set the transform origin to the center of the control (0.5, 0.5).
            RenderTransformOrigin = new Point(0.5, 0.5);

            IsManipulationEnabled = true;
            ManipulationDelta += ZoomableImage_ManipulationDelta;

            MouseLeftButtonDown += Image_MouseLeftButtonDown;
            MouseLeftButtonUp += Image_MouseLeftButtonUp;
            MouseMove += Image_MouseMove;
            MouseWheel += Image_MouseWheel;
            SizeChanged += ZoomableImage_SizeChanged;
        }

        private void ZoomableImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClampTranslation();
        }

        private void ClampTranslation()
        {
            if (Source == null) return;

            // Get the parent container (Grid) to know the true size of the "window"
            var parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
            if (parent == null) return;

            double viewportX = parent.ActualWidth;
            double viewportY = parent.ActualHeight;

            // ActualWidth/Height to base, scaled image size (thanks to Stretch=Uniform)
            double baseImageX = this.ActualWidth;
            double baseImageY = this.ActualHeight;

            double scaledImageX = baseImageX * currentScale;
            double scaledImageY = baseImageY * currentScale;

            double minX, maxX, minY, maxY;

            // If the scaled image is smaller than the window, don't allow panning
            // (XAML already centers it)
            if (scaledImageX <= viewportX)
            {
                minX = 0;
                maxX = 0;
            }
            else
            {
                // The image is wider: limit panning
                // Calculate the maximum offset from the center
                maxX = (scaledImageX - baseImageX) / 2;
                minX = -maxX;
            }

            if (scaledImageY <= viewportY)
            {
                minY = 0;
                maxY = 0;
            }
            else
            {
                maxY = (scaledImageY - baseImageY) / 2;
                minY = -maxY;
            }

            // Apply clamping
            translateTransform.X = Math.Min(Math.Max(translateTransform.X, minX), maxX);
            translateTransform.Y = Math.Min(Math.Max(translateTransform.Y, minY), maxY);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lastDragPoint = e.GetPosition(this);
            Mouse.SetCursor(Cursors.Hand);
            Mouse.OverrideCursor = Cursors.Hand;
            Mouse.UpdateCursor();
            CaptureMouse();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lastDragPoint = null;
            Mouse.OverrideCursor = null;
            Mouse.UpdateCursor();
            ReleaseMouseCapture();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue && IsMouseCaptured)
            {
                double multiplier;
                if (currentScale > 4)
                    multiplier = 4.0;
                else if (currentScale > 1.0)
                    multiplier = Math.Round(currentScale, 1);
                else
                    multiplier = 1.0;

                Point currentPoint = e.GetPosition(this);
                Vector delta = Point.Subtract(currentPoint, lastDragPoint.Value) * multiplier;

                translateTransform.X += delta.X;
                translateTransform.Y += delta.Y;

                ClampTranslation();
                lastDragPoint = currentPoint;
            }
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Source == null) return;

            double zoomFactor = e.Delta > 0 ? _config.MouseWheelZoomStepFactor : 1 / _config.MouseWheelZoomStepFactor;
            double newScale = currentScale * zoomFactor;

            if (newScale < _config.MinScale) newScale = _config.MinScale;
            if (newScale > _config.MaxScale) newScale = _config.MaxScale;

            if (Math.Abs(newScale - currentScale) < 0.001) return;

            Point mousePosition = e.GetPosition(this);

            // Position relative to center of control
            Point relativeMouse = new Point(
                mousePosition.X - (ActualWidth / 2),
                mousePosition.Y - (ActualHeight / 2));

            double oldTranslateX = translateTransform.X;
            double oldTranslateY = translateTransform.Y;
            double oldScale = currentScale;

            currentScale = newScale;
            scaleTransform.ScaleX = newScale;
            scaleTransform.ScaleY = newScale;

            // Math to keep the mouse position stable
            translateTransform.X = relativeMouse.X - (relativeMouse.X - oldTranslateX) * (newScale / oldScale);
            translateTransform.Y = relativeMouse.Y - (relativeMouse.Y - oldTranslateY) * (newScale / oldScale);

            ClampTranslation();
        }

        private void ZoomableImage_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (Source == null) return;

            double zoomFactor = e.DeltaManipulation.Scale.X;
            double newScale = currentScale * zoomFactor;

            if (newScale < _config.MinScale) newScale = _config.MinScale;
            if (newScale > _config.MaxScale) newScale = _config.MaxScale;

            if (Math.Abs(newScale - currentScale) < 0.001) return;

            Point center = e.ManipulationOrigin;

            // Position relative to center of control
            Point relativeCenter = new Point(
                center.X - (ActualWidth / 2),
                center.Y - (ActualHeight / 2));

            double oldTranslateX = translateTransform.X;
            double oldTranslateY = translateTransform.Y;
            double oldScale = currentScale;

            currentScale = newScale;
            scaleTransform.ScaleX = newScale;
            scaleTransform.ScaleY = newScale;

            // Apply zoom...
            translateTransform.X = relativeCenter.X - (relativeCenter.X - oldTranslateX) * (newScale / oldScale);
            translateTransform.Y = relativeCenter.Y - (relativeCenter.Y - oldTranslateY) * (newScale / oldScale);

            // ...then add gesture translation
            translateTransform.X += e.DeltaManipulation.Translation.X;
            translateTransform.Y += e.DeltaManipulation.Translation.Y;

            ClampTranslation();
            e.Handled = true;
        }
    }
}