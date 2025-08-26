using System.IO;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace QuickViewFile.Helpers
{
    public static class FileContentReader
    {
        /// <summary>
        /// Dynamically read content of selected file (it's "workaround" for very slow WPF normal approach)
        /// </summary>
        public static void StartDynamicFileRead(TextBox textBox, string filePath)
        {
            Task.Run(() =>
            {
                long lastPosition = 0;
                while (File.Exists(filePath))
                {
                    try
                    {
                        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            if (fs.Length > lastPosition)
                            {
                                fs.Seek(lastPosition, SeekOrigin.Begin);
                                using (var sr = new StreamReader(fs))
                                {
                                    string? line;
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        string capturedLine = line;
                                        textBox.Dispatcher.Invoke(() =>
                                        {
                                            textBox.AppendText(capturedLine + "\r\n");
                                            textBox.ScrollToEnd();
                                        }, DispatcherPriority.Background);
                                    }
                                    lastPosition = fs.Position;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        textBox.Text += ex.StackTrace;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            });
        }
    }
}