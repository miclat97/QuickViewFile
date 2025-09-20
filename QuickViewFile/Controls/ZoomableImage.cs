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
        private TranslateTransform translateTransform = new TranslateTransform();
        private ScaleTransform scaleTransform = new ScaleTransform();
        private TransformGroup transformGroup = new TransformGroup();

        public ZoomableImage()
        {
            this.UseLayoutRounding = true;
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            this.ClipToBounds = true;

            _config = ConfigHelper.LoadConfig();

            this.RenderTransform = transformGroup;
            this.RenderTransformOrigin = new Point((this.ActualHeight / 2), (this.ActualWidth / 2)); //by default picture is centered in control

            this.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            this.MouseLeftButtonUp += Image_MouseLeftButtonUp;
            this.MouseMove += Image_MouseMove;
            this.MouseWheel += Image_MouseWheel;
            this.SizeChanged += ZoomableImage_SizeChanged;
        }

        private void ZoomableImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClampTranslation();
        }

        private void ClampTranslation()
        {
            if (this.Source == null) return;

            double imageWidth = this.Source.Width * currentScale;
            double imageHeight = this.Source.Height * currentScale;
            double viewportWidth = this.ActualWidth;
            double viewportHeight = this.ActualHeight;

            double maxX = Math.Max((imageWidth - viewportWidth), 0);
            double maxY = Math.Max((imageHeight - viewportHeight), 0);

            // Clamp so the image cannot be dragged outside the visible area
            translateTransform.X = Math.Min(Math.Max(translateTransform.X, -maxX), maxX);
            translateTransform.Y = Math.Min(Math.Max(translateTransform.Y, -maxY), maxY);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lastDragPoint = e.GetPosition(this);
            this.CaptureMouse();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lastDragPoint = null;
            this.ReleaseMouseCapture();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue && this.IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(this);
                Vector delta = Point.Subtract(currentPoint, lastDragPoint.Value);

                translateTransform.X += delta.X;
                translateTransform.Y += delta.Y;

                ClampTranslation();
                lastDragPoint = currentPoint;
            }
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePosition = e.GetPosition(this);
            double zoomFactor = e.Delta > 0 ? _config.MouseWheelZoomStepFactor : 1 / _config.MouseWheelZoomStepFactor;

            double newScale = currentScale * zoomFactor;
            if (newScale < _config.MinScale)
                newScale = _config.MinScale;
            if (newScale > _config.MaxScale)
                newScale = _config.MaxScale;

            if (Math.Abs(newScale - currentScale) < 0.001)
                return;

            // Calculate point from where we will scale
            Point relativePoint = new Point(
                mousePosition.X / this.ActualWidth,
                mousePosition.Y / this.ActualHeight);

            // Save current position before scalling
            double absoluteX = mousePosition.X * currentScale + translateTransform.X;
            double absoluteY = mousePosition.Y * currentScale + translateTransform.Y;

            // Scale
            scaleTransform.ScaleX = newScale;
            scaleTransform.ScaleY = newScale;

            // Calculate new translation to keep the point under the mouse
            translateTransform.X = absoluteX - mousePosition.X * newScale;
            translateTransform.Y = absoluteY - mousePosition.Y * newScale;

            currentScale = newScale;
            ClampTranslation();
        }
    }
}