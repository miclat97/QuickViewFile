import re

with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'r') as f:
    text = f.read()

# Make RefreshFiles public so MainWindow.xaml.cs can call it
text = text.replace('private void RefreshFiles(string? fileToSelect = null)', 'public void RefreshFiles(string? fileToSelect = null)')

# Expose FolderPath
text = text.replace('private string _folderPath;', 'private string _folderPath;\n        public string FolderPath => _folderPath;')

# Update DirectoryInfo parsing
dir_replace = '''                foreach (DirectoryInfo folder in foldersInDirectory)
                {
                    ActiveListItems.Add(new ItemList
                    {
                        Name = folder.Name,
                        Size = "",
                        FullPath = folder.FullName,
                        IsDirectory = true,
                        LastModified = folder.LastWriteTime,
                        LastModifiedString = folder.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                        FileContentModel = new FileContentModel()
                    });
                }'''
text = re.sub(r'foreach \(DirectoryInfo folder in foldersInDirectory\)\s*\{\s*ActiveListItems\.Add\(new ItemList\s*\{\s*Name = folder\.Name,\s*Size = "",\s*FullPath = folder\.FullName,\s*IsDirectory = true,\s*FileContentModel = new FileContentModel\(\)\s*\}\);\s*\}', dir_replace, text)

file_replace = '''                foreach (FileInfo file in filesInDirectory)
                {
                    ActiveListItems.Add(new ItemList
                    {
                        Name = file.Name,
                        Size = Math.Round((file.Length / 1024.0), MidpointRounding.ToPositiveInfinity).ToString(),
                        SizeBytes = file.Length,
                        FullPath = file.FullName,
                        IsDirectory = false,
                        LastModified = file.LastWriteTime,
                        LastModifiedString = file.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                        FileContentModel = new FileContentModel()
                    });
                }'''
text = re.sub(r'foreach \(FileInfo file in filesInDirectory\)\s*\{\s*ActiveListItems\.Add\(new ItemList\s*\{\s*Name = file\.Name,\s*Size = Math\.Round\(\(file\.Length / 1024\.0\), MidpointRounding\.ToPositiveInfinity\)\.ToString\(\),\s*FullPath = file\.FullName,\s*IsDirectory = false,\s*FileContentModel = new FileContentModel\(\)\s*\}\);\s*\}', file_replace, text)

with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'w') as f:
    f.write(text)
