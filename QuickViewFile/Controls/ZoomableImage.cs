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

        // 1. Monitoruje zmianê w³aœciwoœci Source
        static ZoomableImage()
        {
            SourceProperty.OverrideMetadata(typeof(ZoomableImage), new FrameworkPropertyMetadata(OnSourceChanged));
        }

        // 2. Wywo³ywane przy zmianie obrazu: Resetuje stan
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ZoomableImage)?.ResetTransforms();
        }

        // 3. Resetuje tylko transformacje. Centrowaniem zajmuje siê XAML.
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
            UseLayoutRounding = true;
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(translateTransform);
            ClipToBounds = true;

            _config = ConfigHelper.LoadConfig();

            RenderTransform = transformGroup;

            // ########## KLUCZOWA POPRAWKA ##########
            // Ustawia punkt transformacji na œrodek kontrolki (0.5, 0.5).
            // Dziêki temu skalowanie dzia³a wzglêdem centrum, a nie lewego górnego rogu.
            RenderTransformOrigin = new Point(0.5, 0.5);
            // ######################################

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
            // Przelicz ograniczenia (np. gdy okno siê powiêkszy)
            ClampTranslation();
        }

        // 4. POPRAWIONA METODA: Logika ograniczania
        private void ClampTranslation()
        {
            if (Source == null) return;

            // Pobierz nadrzêdny kontener (Grid), aby poznaæ prawdziwy rozmiar "okna"
            var parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
            if (parent == null) return;

            double viewportX = parent.ActualWidth;
            double viewportY = parent.ActualHeight;

            // ActualWidth/Height to bazowy, przeskalowany rozmiar obrazu (dziêki Stretch=Uniform)
            double baseImageX = this.ActualWidth;
            double baseImageY = this.ActualHeight;

            double scaledImageX = baseImageX * currentScale;
            double scaledImageY = baseImageY * currentScale;

            double minX, maxX, minY, maxY;

            // Jeœli przeskalowany obraz jest mniejszy ni¿ okno, nie pozwalaj na przesuwanie
            // (XAML ju¿ go centruje)
            if (scaledImageX <= viewportX)
            {
                minX = 0;
                maxX = 0;
            }
            else
            {
                // Obraz jest szerszy: ogranicz przesuwanie
                // Obliczamy maksymalne przesuniêcie od œrodka
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

            // Zastosuj ograniczenia
            translateTransform.X = Math.Min(Math.Max(translateTransform.X, minX), maxX);
            translateTransform.Y = Math.Min(Math.Max(translateTransform.Y, minY), maxY);
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

        // 5. POPRAWIONA METODA: Zoom-do-myszy dla RenderTransformOrigin(0.5, 0.5)
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Source == null) return;

            double zoomFactor = e.Delta > 0 ? _config.MouseWheelZoomStepFactor : 1 / _config.MouseWheelZoomStepFactor;
            double newScale = currentScale * zoomFactor;

            if (newScale < _config.MinScale) newScale = _config.MinScale;
            if (newScale > _config.MaxScale) newScale = _config.MaxScale;

            if (Math.Abs(newScale - currentScale) < 0.001) return;

            Point mousePosition = e.GetPosition(this);

            // Pozycja myszy wzglêdem œrodka kontrolki
            Point relativeMouse = new Point(
                mousePosition.X - (ActualWidth / 2),
                mousePosition.Y - (ActualHeight / 2));

            double oldTranslateX = translateTransform.X;
            double oldTranslateY = translateTransform.Y;
            double oldScale = currentScale;

            currentScale = newScale;
            scaleTransform.ScaleX = newScale;
            scaleTransform.ScaleY = newScale;

            // Matematyka pozwalaj¹ca utrzymaæ punkt pod mysz¹
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

            // Pozycja gestu wzglêdem œrodka
            Point relativeCenter = new Point(
                center.X - (ActualWidth / 2),
                center.Y - (ActualHeight / 2));

            double oldTranslateX = translateTransform.X;
            double oldTranslateY = translateTransform.Y;
            double oldScale = currentScale;

            currentScale = newScale;
            scaleTransform.ScaleX = newScale;
            scaleTransform.ScaleY = newScale;

            // Zastosuj zoom...
            translateTransform.X = relativeCenter.X - (relativeCenter.X - oldTranslateX) * (newScale / oldScale);
            translateTransform.Y = relativeCenter.Y - (relativeCenter.Y - oldTranslateY) * (newScale / oldScale);

            // ...a nastêpnie dodaj przesuniêcie z gestu
            translateTransform.X += e.DeltaManipulation.Translation.X;
            translateTransform.Y += e.DeltaManipulation.Translation.Y;

            ClampTranslation();
            e.Handled = true;
        }
    }
}