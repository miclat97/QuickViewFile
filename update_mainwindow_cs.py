import re

with open('QuickViewFile/MainWindow.xaml.cs', 'r') as f:
    text = f.read()

# Replace HQButton_Click with SettingsButton_Click
text = text.replace(
'''        private void HQButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm)
            {
                if (vm.Config.BitmapScalingMode == "Fant")
                {
                    vm.Config.BitmapScalingMode = "Linear";
                    HQButton.FontWeight = FontWeights.Normal;
                }
                else
                {
                    vm.Config.BitmapScalingMode = "Fant";
                    HQButton.FontWeight = FontWeights.Bold;
                }
                ConfigHelper.SaveConfig(vm.Config);
            }
        }''',
'''        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            if (settingsWindow.ShowDialog() == true)
            {
                if (DataContext is FilesListViewModel vm)
                {
                    vm.Config = ConfigHelper.LoadConfig();
                }
            }
        }''')


file_operations = '''
        // --- File Operations ---
        private enum FileOperation { None, Copy, Move }
        private FileOperation _currentOperation = FileOperation.None;
        private List<string> _clipboardFiles = new List<string>();

        private void MoveFiles_Click(object sender, RoutedEventArgs e)
        {
            _currentOperation = FileOperation.Move;
            UpdateClipboardFiles();
        }

        private void CopyFiles_Click(object sender, RoutedEventArgs e)
        {
            _currentOperation = FileOperation.Copy;
            UpdateClipboardFiles();
        }

        private void UpdateClipboardFiles()
        {
            if (FilesListView.SelectedItems.Count > 0)
            {
                _clipboardFiles.Clear();
                foreach (ItemList item in FilesListView.SelectedItems)
                {
                    if (!item.IsDirectory && item.Name != "..")
                    {
                        _clipboardFiles.Add(item.FullPath);
                    }
                }
                if (_clipboardFiles.Count > 0)
                {
                    PasteButton.Visibility = Visibility.Visible;
                    PasteButton.Content = $"Paste ({_clipboardFiles.Count} files)";
                }
            }
        }

        private void DeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (FilesListView.SelectedItems.Count > 0 && DataContext is FilesListViewModel vm)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {FilesListView.SelectedItems.Count} items?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    foreach (ItemList item in FilesListView.SelectedItems.Cast<ItemList>().ToList())
                    {
                        if (!item.IsDirectory && item.Name != "..")
                        {
                            try
                            {
                                File.Delete(item.FullPath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Failed to delete {item.Name}: {ex.Message}");
                            }
                        }
                    }
                    vm.RefreshFiles();
                }
            }
        }

        private void PasteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (_clipboardFiles.Count > 0 && DataContext is FilesListViewModel vm)
            {
                string targetDir = vm.FolderPath;
                foreach (string file in _clipboardFiles)
                {
                    try
                    {
                        string fileName = Path.GetFileName(file);
                        string destFile = Path.Combine(targetDir, fileName);

                        if (_currentOperation == FileOperation.Copy)
                        {
                            File.Copy(file, destFile, true);
                        }
                        else if (_currentOperation == FileOperation.Move)
                        {
                            if (file != destFile)
                            {
                                File.Move(file, destFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Operation failed for {file}: {ex.Message}");
                    }
                }

                if (_currentOperation == FileOperation.Move)
                {
                    _clipboardFiles.Clear();
                    _currentOperation = FileOperation.None;
                    PasteButton.Visibility = Visibility.Collapsed;
                }
                vm.RefreshFiles();
            }
        }

        // --- Column Sorting ---
        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null && headerClicked.Tag != null)
            {
                string propertyName = headerClicked.Tag.ToString();

                ListSortDirection direction;
                if (headerClicked != _lastHeaderClicked)
                {
                    direction = ListSortDirection.Ascending;
                }
                else
                {
                    if (_lastDirection == ListSortDirection.Ascending)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = ListSortDirection.Ascending;
                    }
                }

                _lastHeaderClicked = headerClicked;
                _lastDirection = direction;

                SortFilesList(propertyName, direction);
            }
        }

        private void SortFilesList(string sortBy, ListSortDirection direction)
        {
            if (DataContext is FilesListViewModel vm)
            {
                var folders = vm.ActiveListItems.Where(x => x.IsDirectory && x.Name != "..").ToList();
                var files = vm.ActiveListItems.Where(x => !x.IsDirectory).ToList();
                var parentDir = vm.ActiveListItems.Where(x => x.Name == "..").FirstOrDefault();

                if (sortBy == "Name")
                {
                    if (direction == ListSortDirection.Ascending)
                    {
                        folders = folders.OrderBy(x => x.Name).ToList();
                        files = files.OrderBy(x => x.Name).ToList();
                    }
                    else
                    {
                        folders = folders.OrderByDescending(x => x.Name).ToList();
                        files = files.OrderByDescending(x => x.Name).ToList();
                    }
                }
                else if (sortBy == "Size")
                {
                    if (direction == ListSortDirection.Ascending)
                        files = files.OrderBy(x => x.SizeBytes).ToList();
                    else
                        files = files.OrderByDescending(x => x.SizeBytes).ToList();
                }
                else if (sortBy == "LastModified")
                {
                    if (direction == ListSortDirection.Ascending)
                    {
                        folders = folders.OrderBy(x => x.LastModified).ToList();
                        files = files.OrderBy(x => x.LastModified).ToList();
                    }
                    else
                    {
                        folders = folders.OrderByDescending(x => x.LastModified).ToList();
                        files = files.OrderByDescending(x => x.LastModified).ToList();
                    }
                }

                vm.ActiveListItems.Clear();
                if (parentDir != null)
                    vm.ActiveListItems.Add(parentDir);

                foreach (var folder in folders)
                    vm.ActiveListItems.Add(folder);

                foreach (var file in files)
                    vm.ActiveListItems.Add(file);
            }
        }
'''

# Insert the file_operations before `private void AppWindow_KeyDown`
text = text.replace('private void AppWindow_KeyDown', file_operations + '\n        private void AppWindow_KeyDown')

with open('QuickViewFile/MainWindow.xaml.cs', 'w') as f:
    f.write(text)
