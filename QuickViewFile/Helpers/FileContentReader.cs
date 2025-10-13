using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuickViewFile.Helpers
{
    public static class FileContentReader
    {
        public unsafe static Span<string> ExtractCleanASCIIStringsFromDirtyString(string dirtyInput, int dirtySize, int? minLength = 3)
        {
            if (string.IsNullOrEmpty(dirtyInput) || minLength <= 0)
            {
                return Span<string>.Empty;
            }

            var stringsASCII = new List<string>();
            var currentString = new StringBuilder();

            char* dirtyInput_firstCSharpPointerInMyLife_xD = (char*)&dirtyInput;

            StringBuilder asciiTempString = new StringBuilder();
            for (int i = 0; i < dirtySize; i++)
            {
                char currentChar = *(dirtyInput_firstCSharpPointerInMyLife_xD + i);
                if (currentChar >= 0 && currentChar <= 126) // ASCII printable characters
                {
                    asciiTempString.Append(currentChar);
                }
                else // if non-printable character is found that means we either reached the end of a string or it's just garbage
                {
                    if (asciiTempString.Length >= minLength)
                    {
                        stringsASCII.Add(asciiTempString.ToString());
                    }
                    asciiTempString.Clear();
                }
            }

            return stringsASCII.ToArray();
        }

        public static async Task<string> ReadTextFileAsync(string filePath, double maxChars)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true); // Async file stream
                {
                    using (var reader = new StreamReader(fileStream, Encoding.UTF8, true, 4096, true)) // Add encoding and buffer size
                    {
                        StringBuilder result = new StringBuilder();
                        char[] buffer = new char[4096]; // Smaller buffer
                        int charsRead;

                        while ((charsRead = await reader.ReadAsync(buffer, 0, Math.Min(buffer.Length, (int)maxChars - result.Length))) > 0 && result.Length < maxChars)
                        {
                            result.Append(buffer, 0, charsRead);
                        }

                        if (result.Length == maxChars)
                        {
                            result.AppendLine("\n[File truncated - too large to display completely]");
                        }

                        return result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string GetOnlyCleanASCIIStrings(string[] inputStrings)
        {
            StringBuilder result = new StringBuilder();
            foreach (var input in inputStrings)
            {
                result.AppendLine(input);
            }

            return result.ToString();
        }
    }
}