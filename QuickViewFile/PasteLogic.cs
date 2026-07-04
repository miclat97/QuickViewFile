using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuickViewFile
{
    public static class PasteLogic
    {
        public static async Task PerformPasteAsync(
            List<string> clipboardFiles,
            string targetDir,
            int currentOperation,
            Window ownerWindow,
            ProgressBar progressBar,
            TextBlock statusText,
            CancellationTokenSource cts,
            Action onComplete)
        {
            if (clipboardFiles.Count == 0) return;

            bool doForAll = false;
            OverwriteAction allAction = OverwriteAction.Skip;
            long copiedBytes = 0;

            await Task.Run(() =>
            {
                long totalBytes = 0;
                foreach (var path in clipboardFiles)
                {
                    if (File.Exists(path))
                        totalBytes += new FileInfo(path).Length;
                    else if (Directory.Exists(path))
                        totalBytes += Helpers.DirectoryOperationHelper.GetDirectorySize(new DirectoryInfo(path));
                }

                foreach (string itemPath in clipboardFiles)
                {
                    if (cts.Token.IsCancellationRequested) break;

                    try
                    {
                        if (!File.Exists(itemPath) && !Directory.Exists(itemPath)) continue;

                        bool isDirectory = Directory.Exists(itemPath);
                        string itemName = Path.GetFileName(itemPath.TrimEnd(Path.DirectorySeparatorChar));
                        string destPath = Path.Combine(targetDir, itemName);

                        Application.Current.Dispatcher.InvokeAsync(() => statusText.Text = $"Processing: {itemName}");

                        if (File.Exists(destPath) || Directory.Exists(destPath))
                        {
                            OverwriteAction action = OverwriteAction.Skip;
                            if (doForAll)
                            {
                                action = allAction;
                            }
                            else
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var overwriteDialog = new OverwriteDialog($"'{itemName}' already exists. What do you want to do?");
                                    overwriteDialog.Owner = ownerWindow;
                                    overwriteDialog.ShowDialog();
                                    action = overwriteDialog.SelectedAction;
                                    doForAll = overwriteDialog.DoForAll;
                                    allAction = action;
                                });
                            }

                            if (action == OverwriteAction.Skip)
                            {
                                if (isDirectory)
                                    copiedBytes += Helpers.DirectoryOperationHelper.GetDirectorySize(new DirectoryInfo(itemPath));
                                else
                                    copiedBytes += new FileInfo(itemPath).Length;

                                Application.Current.Dispatcher.InvokeAsync(() => { if (totalBytes > 0) progressBar.Value = (double)copiedBytes / totalBytes * 100; });
                                continue;
                            }
                            else if (action == OverwriteAction.Replace)
                            {
                                if (File.Exists(destPath)) File.Delete(destPath);
                                else if (Directory.Exists(destPath)) Helpers.DirectoryOperationHelper.DeleteDirectoryRecursive(destPath);
                            }
                            else if (action == OverwriteAction.AutoRename)
                            {
                                destPath = Helpers.DirectoryOperationHelper.GetAutoRenamedPath(destPath);
                            }
                        }


                        int lastProgressPercentage = -1;
                        DateTime lastUpdateTime = DateTime.Now;

                        Action<long> progressCallback = (bytes) =>
                        {
                            copiedBytes += bytes;
                            if (totalBytes > 0)
                            {
                                int percentage = (int)((double)copiedBytes / totalBytes * 100);
                                if (percentage > lastProgressPercentage || (DateTime.Now - lastUpdateTime).TotalMilliseconds > 100)
                                {
                                    lastProgressPercentage = percentage;
                                    lastUpdateTime = DateTime.Now;
                                    Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        progressBar.Value = percentage;
                                    });
                                }
                            }
                        };


                        if (currentOperation == 1) // Copy
                        {
                            if (isDirectory)
                                Helpers.DirectoryOperationHelper.CopyDirectoryRecursive(itemPath, destPath, true, progressCallback, cts.Token);
                            else
                                Helpers.DirectoryOperationHelper.CopyFileSafe(itemPath, destPath, progressCallback, cts.Token);
                        }
                        else if (currentOperation == 2) // Move
                        {
                            if (isDirectory)
                                Helpers.DirectoryOperationHelper.MoveDirectoryRecursive(itemPath, destPath, true, progressCallback, cts.Token);
                            else
                            {
                                if (itemPath != destPath)
                                    Helpers.DirectoryOperationHelper.MoveFile(itemPath, destPath, progressCallback, cts.Token);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        // Skip failed items
                    }
                }
            });

            onComplete();
        }
    }
}