using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QuickViewFile.Controls
{
    public class ZoomableMediaElement : MediaElement
    {
        private Point? lastDragPoint;
        private double currentScale = 1.0;
        private readonly TranslateTransform translateTransform = new TranslateTransform();
        private readonly ScaleTransform scaleTransform = new ScaleTransform();
        private readonly TransformGroup transformGroup = new TransformGroup();

        public ZoomableMediaElement()
        {
            UseLayoutRounding = true;
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            ClipToBounds = true;

            RenderTransform = transformGroup;
            RenderTransformOrigin = new Point(0, 0);

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
            double mediaElementWidth = NaturalVideoWidth * currentScale;
            double mediaElementHeight = NaturalVideoHeight * currentScale;
            double viewportWidth = ActualWidth;
            double viewportHeight = ActualHeight;

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
            CaptureMouse();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lastDragPoint = null;
            ReleaseMouseCapture();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue && IsMouseCaptured)
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
                mousePosition.X / ActualWidth,
                mousePosition.Y / ActualHeight);

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
