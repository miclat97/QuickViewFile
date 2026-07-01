using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace QuickViewFile.Helpers
{
    public static class LoadImageWithOrientationHelper
    {
        public static BitmapSource LoadImageWithOrientation(string path)
        {
            BitmapSource bitmapSource = null;
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.OnLoad);
                BitmapFrame bitmapFrame = decoder.Frames[0];
                bitmapSource = bitmapFrame;

                BitmapMetadata? metadata = bitmapFrame.Metadata as BitmapMetadata;
                if (metadata != null && metadata.ContainsQuery("System.Photo.Orientation"))
                {
                    object? orientationValue = metadata.GetQuery("System.Photo.Orientation");
                    if (orientationValue != null)
                    {
                        try
                        {
                            int orientation = Convert.ToInt32(orientationValue);
                            Transform transform = null;

                            switch (orientation)
                            {
                                case 6:
                                    transform = new RotateTransform(90);
                                    break;
                                case 8:
                                    transform = new RotateTransform(270);
                                    break;
                                case 3:
                                    transform = new RotateTransform(180);
                                    break;
                            }

                            if (transform != null)
                            {
                                TransformedBitmap transformedBitmap = new TransformedBitmap(bitmapSource, transform);
                                transformedBitmap.Freeze();
                                bitmapSource = transformedBitmap;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error applying orientation: {ex.Message}");
                        }
                    }
                }
                bitmapSource.Freeze();
            }
            return bitmapSource;
        }

        public static BitmapSource LoadThumbnailWithOrientation(string path, int decodePixelWidth)
        {
            BitmapSource bitmapSource = null;
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Decode to a specific width to save RAM
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmapImage.StreamSource = stream;
                bitmapImage.DecodePixelWidth = decodePixelWidth;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                bitmapSource = bitmapImage;

                // Try to read metadata from the original stream without decoding pixels again
                stream.Position = 0;
                BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                BitmapMetadata? metadata = decoder.Frames[0].Metadata as BitmapMetadata;
                if (metadata != null && metadata.ContainsQuery("System.Photo.Orientation"))
                {
                    object? orientationValue = metadata.GetQuery("System.Photo.Orientation");
                    if (orientationValue != null)
                    {
                        try
                        {
                            int orientation = Convert.ToInt32(orientationValue);
                            Transform transform = null;

                            switch (orientation)
                            {
                                case 6:
                                    transform = new RotateTransform(90);
                                    break;
                                case 8:
                                    transform = new RotateTransform(270);
                                    break;
                                case 3:
                                    transform = new RotateTransform(180);
                                    break;
                            }

                            if (transform != null)
                            {
                                TransformedBitmap transformedBitmap = new TransformedBitmap(bitmapSource, transform);
                                transformedBitmap.Freeze();
                                bitmapSource = transformedBitmap;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error applying orientation: {ex.Message}");
                        }
                    }
                }
                bitmapSource.Freeze();
            }
            return bitmapSource;
        }
    }
}
