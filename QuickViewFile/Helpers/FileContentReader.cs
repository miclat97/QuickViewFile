using System.IO;
using System.Text;

namespace QuickViewFile.Helpers
{
    public static class FileContentReader
    {
        public static async Task<string> ReadTextFileAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                StringBuilder resultString = new StringBuilder();

                using (var reader = new StreamReader(filePath))
                {
                    char[] charsBuffer = new char[8192];

                    while (reader.Read(charsBuffer, 0, 8192) != 0)
                    {
                        resultString.Append(ReplaceAllNonASCIICharacters(new string(charsBuffer)));
                    }
                }

                return resultString.ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private static string ReplaceAllNonASCIICharacters(string inputString)
        {

            StringBuilder resultStringBuilder = new StringBuilder();

            for (int i = 0; i < inputString.Length; i++)
            {
                if (inputString[i] < 128)
                {
                    resultStringBuilder.Append((char)inputString[i]);
                }
            }
            return resultStringBuilder.ToString();
        }
    }
}