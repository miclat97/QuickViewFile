using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace QuickViewFile.Helpers
{
    public static class DirectoryOperationHelper
    {
        public static void DeleteDirectoryRecursive(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        private static string GetDriveLetter(string path)
        {
            return Path.GetPathRoot(path)?.ToUpperInvariant() ?? string.Empty;
        }

        public static void CopyDirectoryRecursive(string sourcePath, string destinationPath, bool excludeAds = true, Action<long>? progressCallback = null, CancellationToken cancellationToken = default)
        {
            var sourceDir = new DirectoryInfo(sourcePath);

            if (!sourceDir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            foreach (var file in sourceDir.GetFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    string destFilePath = Path.Combine(destinationPath, file.Name);
                    CopyFileSafe(file.FullName, destFilePath, progressCallback, cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to copy file '{file.Name}': {ex.Message}", ex);
                }
            }

            foreach (var directory in sourceDir.GetDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();
                string destDirPath = Path.Combine(destinationPath, directory.Name);
                CopyDirectoryRecursive(directory.FullName, destDirPath, excludeAds, progressCallback, cancellationToken);
            }
        }

        public static void MoveDirectoryRecursive(string sourcePath, string destinationPath, bool excludeAds = true, Action<long>? progressCallback = null, CancellationToken cancellationToken = default)
        {
            var sourceDir = new DirectoryInfo(sourcePath);

            if (!sourceDir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

            string targetDir = Path.GetDirectoryName(destinationPath) ?? throw new ArgumentException("Invalid destination path", nameof(destinationPath));

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            if (GetDriveLetter(sourcePath) == GetDriveLetter(destinationPath))
            {
                try
                {
                    Directory.Move(sourcePath, destinationPath);
                    progressCallback?.Invoke(GetDirectorySize(new DirectoryInfo(destinationPath)));
                    return;
                }
                catch
                {
                }
            }

            CopyDirectoryRecursive(sourcePath, destinationPath, excludeAds, progressCallback, cancellationToken);
            try
            {
                Directory.Delete(sourcePath, true);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to delete source directory after copy: {ex.Message}", ex);
            }
        }

        public static void MoveFile(string sourcePath, string destinationPath, Action<long>? progressCallback = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    if (GetDriveLetter(sourcePath) == GetDriveLetter(destinationPath))
                    {
                        if (File.Exists(destinationPath))
                            File.Delete(destinationPath);
                        File.Move(sourcePath, destinationPath);
                        progressCallback?.Invoke(new FileInfo(destinationPath).Length);
                    }
                    else
                    {
                        CopyFileSafe(sourcePath, destinationPath, progressCallback, cancellationToken);
                        File.Delete(sourcePath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to move file: {ex.Message}", ex);
            }
        }

        public static void CopyFileSafe(string sourcePath, string destinationPath, Action<long>? progressCallback = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    using (var sourceStream = File.OpenRead(sourcePath))
                    using (var destStream = File.Create(destinationPath))
                    {
                        byte[] buffer = new byte[81920];
                        int bytesRead;
                        while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            destStream.Write(buffer, 0, bytesRead);
                            progressCallback?.Invoke(bytesRead);
                        }
                    }

                    File.SetAttributes(destinationPath, File.GetAttributes(sourcePath));
                    File.SetLastWriteTime(destinationPath, File.GetLastWriteTime(sourcePath));
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to copy file safely: {ex.Message}", ex);
            }
        }

        public static long GetDirectorySize(DirectoryInfo d)
        {
            long size = 0;
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += GetDirectorySize(di);
            }
            return size;
        }

        public static string GetAutoRenamedPath(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path)) return path;

            string directory = Path.GetDirectoryName(path) ?? string.Empty;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            int count = 1;
            string newFullPath;
            do
            {
                string newName = $"{fileNameWithoutExtension}({count}){extension}";
                newFullPath = Path.Combine(directory, newName);
                count++;
            } while (File.Exists(newFullPath) || Directory.Exists(newFullPath));

            return newFullPath;
        }
    }
}