using System.Text;

namespace QuickViewFile.Extensions
{
    public static class StringExtension
    {
        private static readonly ASCIIEncoding asciiEncoding = new ASCIIEncoding();

        public static string ToAscii(this string dirty)
        {
            byte[] bytes = asciiEncoding.GetBytes(dirty);
            string clean = asciiEncoding.GetString(bytes);
            return clean;
        }
    }
}
