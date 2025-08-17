using System.IO;
using System.Threading.Tasks;

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
    }
}