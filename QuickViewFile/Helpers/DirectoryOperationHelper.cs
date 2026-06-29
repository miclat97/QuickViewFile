using System;
using System.IO;
using System.Linq;

namespace QuickViewFile.Helpers
{
    public static class DirectoryOperationHelper
    {
        /// <summary>
        /// Kopiuje folder rekurencyjnie, zachowując strukturę, ignorując strumienie alternatywne (ADS)
        /// </summary>
        public static void CopyDirectoryRecursive(string sourcePath, string destinationPath, bool excludeAds = true)
        {
            var sourceDir = new DirectoryInfo(sourcePath);

            if (!sourceDir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

            // Utwórz folder docelowy jeśli go nie ma
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // Kopiuj pliki z bieżącego poziomu - OMIJ ADS
            foreach (var file in sourceDir.GetFiles())
            {
                try
                {
                    string destFilePath = Path.Combine(destinationPath, file.Name);

                    // Kopiuj tylko główny stream (bez ADS)
                    using (var sourceStream = File.OpenRead(file.FullName))
                    using (var destStream = File.Create(destFilePath))
                    {
                        sourceStream.CopyTo(destStream);
                    }

                    // Przywróć atrybuty pliku
                    File.SetAttributes(destFilePath, File.GetAttributes(file.FullName));

                    // Przywróć czas modyfikacji
                    File.SetLastWriteTime(destFilePath, File.GetLastWriteTime(file.FullName));
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to copy file '{file.Name}': {ex.Message}", ex);
                }
            }

            // Rekurencyjnie kopiuj podfoldery, zachowując strukturę
            foreach (var directory in sourceDir.GetDirectories())
            {
                string destDirPath = Path.Combine(destinationPath, directory.Name);
                CopyDirectoryRecursive(directory.FullName, destDirPath, excludeAds);
            }
        }

        /// <summary>
        /// Przenosi folder rekurencyjnie, zachowując strukturę, ignorując strumienie alternatywne (ADS)
        /// </summary>
        public static void MoveDirectoryRecursive(string sourcePath, string destinationPath, bool excludeAds = true)
        {
            var sourceDir = new DirectoryInfo(sourcePath);

            if (!sourceDir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

            // destinationPath powinno zawierać pełną ścieżkę z nazwą docelowego folderu
            string targetDir = Path.GetDirectoryName(destinationPath) ?? throw new ArgumentException("Invalid destination path", nameof(destinationPath));

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // Jeśli cel już istnieje, usuń go
            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            // Jeśli dysk źródłowy == dysk docelowy, spróbuj szybkiego przeniesienia
            if (GetDriveLetter(sourcePath) == GetDriveLetter(destinationPath))
            {
                try
                {
                    Directory.Move(sourcePath, destinationPath);
                    return;
                }
                catch
                {
                    // Jeśli to się nie powiedzie, fallback na kopiowanie + usuwanie
                }
            }

            // Cross-drive move: kopiuj strukturę i usuń źródło
            CopyDirectoryRecursive(sourcePath, destinationPath, excludeAds);
            try
            {
                Directory.Delete(sourcePath, true);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to delete source directory after copy: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Wyodrębnia literę dysku z ścieżki
        /// </summary>
        private static string GetDriveLetter(string path)
        {
            return Path.GetPathRoot(path)?.ToUpperInvariant() ?? string.Empty;
        }

        /// <summary>
        /// Usuwa folder rekurencyjnie (łącznie z zawartością)
        /// </summary>
        public static void DeleteDirectoryRecursive(string directoryPath)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to delete directory: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Przenosi plik pojedynczy (również bezpieczny dla ADS - nie kopiuje)
        /// </summary>
        public static void MoveFile(string sourcePath, string destinationPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    // Jeśli cel istnieje, usuń go
                    if (File.Exists(destinationPath))
                    {
                        File.Delete(destinationPath);
                    }

                    // Jeśli na tym samym dysku, użyj Move
                    if (GetDriveLetter(sourcePath) == GetDriveLetter(destinationPath))
                    {
                        File.Move(sourcePath, destinationPath, true);
                    }
                    else
                    {
                        // Cross-drive: kopiuj + usuń
                        CopyFileSafe(sourcePath, destinationPath);
                        File.Delete(sourcePath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to move file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Kopiuje plik bezpiecznie (bez ADS)
        /// </summary>
        public static void CopyFileSafe(string sourcePath, string destinationPath)
        {
            try
            {
                if (File.Exists(sourcePath))
                {
                    using (var sourceStream = File.OpenRead(sourcePath))
                    using (var destStream = File.Create(destinationPath))
                    {
                        sourceStream.CopyTo(destStream);
                    }

                    // Przywróć atrybuty i czas modyfikacji
                    File.SetAttributes(destinationPath, File.GetAttributes(sourcePath));
                    File.SetLastWriteTime(destinationPath, File.GetLastWriteTime(sourcePath));
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to copy file safely: {ex.Message}", ex);
            }
        }
    }
}