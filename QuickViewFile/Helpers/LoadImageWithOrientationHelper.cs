using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace QuickViewFile.Helpers
{
    public static class LoadImageWithOrientationHelper
    {
        public static BitmapSource LoadImageWithOrientation(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var decoder = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

                var frame = decoder.Frames[0];
                var metadata = frame.Metadata as BitmapMetadata;

                int orientation = 1;
                if (metadata != null && metadata.ContainsQuery("System.Photo.Orientation"))
                {
                    orientation = Convert.ToInt32(metadata.GetQuery("System.Photo.Orientation"));
                }

                BitmapSource source = frame;


                switch (orientation)
                {
                    case 3: // 180 degrees
                        source = new TransformedBitmap(source, new RotateTransform(180));
                        break;
                    case 6: // 90 degrees right
                        source = new TransformedBitmap(source, new RotateTransform(90));
                        break;
                    case 8: // 90 degrees left
                        source = new TransformedBitmap(source, new RotateTransform(270));
                        break;
                }

                source.Freeze();
                return source;
            }
        }
    }
}
