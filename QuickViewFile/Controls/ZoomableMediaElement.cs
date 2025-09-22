using QuickViewFile.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuickViewFile.Controls
{
    public class ZoomableMediaElement : MediaElement
    {
        private readonly ConfigModel _config;
        private Point? lastDragPoint;
        private double currentScale = 1.0;
        private TranslateTransform translateTransform = new TranslateTransform();
        private ScaleTransform scaleTransform = new ScaleTransform();
        private TransformGroup transformGroup = new TransformGroup();

        public ZoomableMediaElement()
        {
            this.UseLayoutRounding = true;
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            this.ClipToBounds = true;

            this.RenderTransform = transformGroup;
            this.RenderTransformOrigin = new Point(0, 0);

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
            double mediaElementWidth = this.NaturalVideoWidth * currentScale;
            double mediaElementHeight = this.NaturalVideoHeight * currentScale;
            double viewportWidth = this.ActualWidth;
            double viewportHeight = this.ActualHeight;

            double maxX = Math.Max((mediaElementWidth - viewportWidth), 0);
            double maxY = Math.Max((mediaElementHeight - viewportHeight), 0);

            maxY *= 2;
            maxX *= 2;

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
            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;

            double newScale = currentScale * zoomFactor;
            if (newScale < 1)
                newScale = 1;
            if (newScale > 100)
                newScale = 100;

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
