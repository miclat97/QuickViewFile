using System.IO;

namespace QuickViewFile.Helpers
{
    public static class FileContentReader
    {
        public static async Task<string?> ReadTextFileAsync(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            var fileInfo = new FileInfo(filePath);
            string fileContent;

            try
            {
                fileContent = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return fileContent;
        }

        public static byte[]? ReadBytesFromFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                return File.ReadAllBytes(filePath);
            }
            catch
            {
                return null;
            }
        }
    }
}