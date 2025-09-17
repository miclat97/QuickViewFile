using System.IO;

namespace QuickViewFile.Helpers
{
    public static class FileContentReader
    {
        public static async Task<string> ReadTextFileAsync(string? filePath, int maxChars = 1_000_000)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return string.Empty;

            try
            {
                using var reader = new StreamReader(filePath);
                var buffer = new char[maxChars];
                var read = await reader.ReadBlockAsync(buffer, 0, maxChars);

                if (read == maxChars)
                {
                    return new string(buffer, 0, read) + "\n\n[File truncated - too large to display completely]";
                }

                return new string(buffer, 0, read);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}