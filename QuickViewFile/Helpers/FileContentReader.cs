using System.IO;

namespace QuickViewFile.Helpers
{
    public static class FileContentReader
    {
        /// <summary>
        /// Reads file m3u/m3u8 and parse valid stream URL
        /// </summary>
        public static string? ExtractStreamUrlFromM3u(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                foreach (string line in File.ReadLines(filePath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string cleanLine = line.Trim();

                    if (cleanLine.StartsWith("#"))
                        continue;


                    if (cleanLine.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||  // both http and https
                        cleanLine.StartsWith("rtsp", StringComparison.OrdinalIgnoreCase) ||
                        cleanLine.StartsWith("rtmp", StringComparison.OrdinalIgnoreCase) ||
                        cleanLine.StartsWith("udp", StringComparison.OrdinalIgnoreCase) ||
                        cleanLine.StartsWith("mms", StringComparison.OrdinalIgnoreCase))
                    {
                        return cleanLine;
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error parsing m3u/m3u8 file!";
            }

            return null;
        }
    }
}