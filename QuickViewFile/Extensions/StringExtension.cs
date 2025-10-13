using System.Text;

namespace System
{
    public static class StringExtension
    {
        private static readonly ASCIIEncoding asciiEncoding = new ASCIIEncoding();

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
    }
}
