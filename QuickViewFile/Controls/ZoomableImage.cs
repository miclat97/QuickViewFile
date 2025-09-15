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
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);

            _config = ConfigHelper.LoadConfig();

            this.RenderTransform = transformGroup;
            this.RenderTransformOrigin = new Point(0.5, 0.5);

            this.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            this.MouseLeftButtonUp += Image_MouseLeftButtonUp;
            this.MouseMove += Image_MouseMove;
            this.MouseWheel += Image_MouseWheel;
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

            // Calculate the point relative to which we will scale
            Point relativePoint = new Point(mousePosition.X / this.ActualWidth, mousePosition.Y / this.ActualHeight);

            // Save current absolute position
            double absoluteX = mousePosition.X * currentScale + translateTransform.X;
            double absoluteY = mousePosition.Y * currentScale + translateTransform.Y;

            // Apply new scale
            scaleTransform.ScaleX = newScale;
            scaleTransform.ScaleY = newScale;

            // Calculate new absolute position after scaling
            translateTransform.X = absoluteX - mousePosition.X * newScale;
            translateTransform.Y = absoluteY - mousePosition.Y * newScale;

            currentScale = newScale;
        }
    }
}