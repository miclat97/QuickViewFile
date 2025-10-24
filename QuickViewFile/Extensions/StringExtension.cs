using System.Text;

namespace System
{
    public static class StringExtension
    {
        private static readonly ASCIIEncoding asciiEncoding = new ASCIIEncoding();
        private static readonly UTF8Encoding utf8Encoding = new UTF8Encoding();

        public unsafe static string ToAscii(this string dirty)
        {
            fixed (char* p = dirty)
            {
                for (int i = 0; i < dirty.Length; i++)
                {
                    if (p[i] > 127) // If character is non-ASCII
                    {
                        p[i] = ' ';
                    }
                }
            }
            return dirty;
        }

        public static string ToUtf8(this string dirty)
        {
            return utf8Encoding.GetString(utf8Encoding.GetBytes(dirty));
        }
    }
}
