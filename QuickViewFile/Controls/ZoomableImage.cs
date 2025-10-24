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

        public ZoomableImage()
        {
            UseLayoutRounding = true;
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            ClipToBounds = true;

            _config = ConfigHelper.LoadConfig();

            RenderTransform = transformGroup;
            RenderTransformOrigin = new Point(0, 0);

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

            double imageX = Source.Width * currentScale;
            double imageY = Source.Height * currentScale;
            double viewportX = ActualWidth;
            double viewportY = ActualHeight;

            // When image is smaller than viewport:
            // - If NOT dragging, center it.
            // - If dragging (mouse captured), do not force-center; allow the user to drag freely.
            if (imageX <= viewportX)
            {
                if (!IsMouseCaptured)
                {
                    translateTransform.X = (viewportX - imageX) / 2;
                }
                // else: leave translateTransform.X as-is while dragging to avoid snap/jitter
            }
            else
            {
                // Image wider than viewport: clamp so edges cannot be dragged beyond view
                translateTransform.X = Math.Min(0, Math.Max(translateTransform.X, viewportX - imageX));
            }

            if (imageY <= viewportY)
            {
                if (!IsMouseCaptured)
                {
                    translateTransform.Y = (viewportY - imageY) / 2;
                }
                // else: leave translateTransform.Y as-is while dragging to avoid snap/jitter
            }
            else
            {
                translateTransform.Y = Math.Min(0, Math.Max(translateTransform.Y, viewportY - imageY));
            }
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
            // After user releases drag, enforce clamping / centering
            ClampTranslation();
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue && IsMouseCaptured)
            {
                Point currentPoint = e.GetPosition(this);

                // If pointer is outside the control bounds, do NOT move the image.
                // Still update lastDragPoint so when the pointer returns we don't get a huge delta/jump.
                if (currentPoint.X < 0 || currentPoint.X > ActualWidth ||
                    currentPoint.Y < 0 || currentPoint.Y > ActualHeight)
                {
                    lastDragPoint = currentPoint;
                    return;
                }

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

        private void ZoomableImage_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            double zoomFactor = e.DeltaManipulation.Scale.X;
            double newScale = currentScale * zoomFactor;

            if (newScale < _config.MinScale)
                newScale = _config.MinScale;
            if (newScale > _config.MaxScale)
                newScale = _config.MaxScale;

            Point center = e.ManipulationOrigin;

            double absoluteX = center.X * currentScale + translateTransform.X;
            double absoluteY = center.Y * currentScale + translateTransform.Y;

            scaleTransform.ScaleX = newScale;
            scaleTransform.ScaleY = newScale;

            translateTransform.X = absoluteX - center.X * newScale;
            translateTransform.Y = absoluteY - center.Y * newScale;

            currentScale = newScale;

            translateTransform.X += e.DeltaManipulation.Translation.X;
            translateTransform.Y += e.DeltaManipulation.Translation.Y;

            ClampTranslation();

            e.Handled = true;
        }

    }
}