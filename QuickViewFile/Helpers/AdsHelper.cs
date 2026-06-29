using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QuickViewFile.Helpers
{
    public static class AdsHelper
    {
        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr FindFirstStreamW(string lpFileName, STREAM_INFO_LEVELS InfoLevel, out WIN32_FIND_STREAM_DATA lpFindStreamData, uint dwFlags);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindNextStreamW(IntPtr hFindStream, out WIN32_FIND_STREAM_DATA lpFindStreamData);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindClose(IntPtr hFindFile);

        private enum STREAM_INFO_LEVELS
        {
            FindStreamInfoStandard,
            FindStreamInfoMaxInfoLevel
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_STREAM_DATA
        {
            public long StreamSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 296)]
            public string cStreamName;
        }

        public class StreamInfo
        {
            public string Name { get; set; } = string.Empty;
            public long Size { get; set; }
        }

        public static List<StreamInfo> GetAlternateDataStreams(string filePath)
        {
            List<StreamInfo> result = new List<StreamInfo>();
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return result;
            }

            WIN32_FIND_STREAM_DATA findStreamData;
            IntPtr hFind = FindFirstStreamW(filePath, STREAM_INFO_LEVELS.FindStreamInfoStandard, out findStreamData, 0);

            if (hFind != new IntPtr(-1) && hFind != IntPtr.Zero)
            {
                try
                {
                    do
                    {
                        if (findStreamData.cStreamName != "::$DATA")
                        {
                            string streamName = findStreamData.cStreamName;
                            if (streamName.StartsWith(":"))
                            {
                                streamName = streamName.Substring(1);
                                int dataIndex = streamName.IndexOf(":$DATA", StringComparison.OrdinalIgnoreCase);
                                if (dataIndex >= 0)
                                {
                                    streamName = streamName.Substring(0, dataIndex);
                                }
                            }
                            if (!string.IsNullOrEmpty(streamName))
                            {
                                result.Add(new StreamInfo { Name = streamName, Size = findStreamData.StreamSize });
                            }
                        }
                    }
                    while (FindNextStreamW(hFind, out findStreamData));
                }
                finally
                {
                    FindClose(hFind);
                }
            }
            return result;
        }
    }
}
