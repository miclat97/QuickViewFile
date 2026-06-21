import re

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

# Replace AppWindow_KeyDown and FilesListView_KeyDown entirely.
appwindow_pattern = r'private void AppWindow_KeyDown\(object sender, KeyEventArgs e\)\s*\{[\s\S]*?private void FilesListView_KeyDown\(object sender, KeyEventArgs e\)\s*\{\s*\}\s*private void StatusBarTextBlock_MouseDown'

replacement = '''private void ExitFullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && vm.SelectedItem?.FullPath is not null)
            {
                MainWindow fullScreen = new MainWindow(vm.SelectedItem.FullPath);
                if (vm.SelectedItem.FileContentModel.VideoMedia is not null)
                {
                    vm.SelectedItem.FileContentModel.VideoMedia.StopForce();
                }
                fullScreen.Show();
                this.Close();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
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
        }

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
        private System.ComponentModel.ListSortDirection _lastDirection = System.ComponentModel.ListSortDirection.Ascending;

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null && headerClicked.Tag != null)
            {
                string propertyName = headerClicked.Tag.ToString();

                System.ComponentModel.ListSortDirection direction;
                if (headerClicked != _lastHeaderClicked)
                {
                    direction = System.ComponentModel.ListSortDirection.Ascending;
                }
                else
                {
                    if (_lastDirection == System.ComponentModel.ListSortDirection.Ascending)
                    {
                        direction = System.ComponentModel.ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = System.ComponentModel.ListSortDirection.Ascending;
                    }
                }

                _lastHeaderClicked = headerClicked;
                _lastDirection = direction;

                SortFilesList(propertyName, direction);
            }
        }

        private void SortFilesList(string sortBy, System.ComponentModel.ListSortDirection direction)
        {
            if (DataContext is FilesListViewModel vm)
            {
                var folders = vm.ActiveListItems.Where(x => x.IsDirectory && x.Name != "..").ToList();
                var files = vm.ActiveListItems.Where(x => !x.IsDirectory).ToList();
                var parentDir = vm.ActiveListItems.Where(x => x.Name == "..").FirstOrDefault();

                if (sortBy == "Name")
                {
                    if (direction == System.ComponentModel.ListSortDirection.Ascending)
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
                    if (direction == System.ComponentModel.ListSortDirection.Ascending)
                        files = files.OrderBy(x => x.SizeBytes).ToList();
                    else
                        files = files.OrderByDescending(x => x.SizeBytes).ToList();
                }
                else if (sortBy == "LastModified")
                {
                    if (direction == System.ComponentModel.ListSortDirection.Ascending)
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

        private void AppWindow_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.F11)
                {
                    if (_filesListViewVisible)
                    {
                        HideUI();
                    }
                    else
                    {
                        ShowUI();
                    }
                    e.Handled = true;
                    return;
                }

                if (DataContext is FilesListViewModel vm)
                {
                    if (e.Key == Key.F4 && vm.SelectedItem?.FullPath is not null)
                    {
                        MainWindow fullScreen = new MainWindow(vm.SelectedItem.FullPath);
                        if (vm.SelectedItem.FileContentModel.VideoMedia is not null)
                        {
                            vm.SelectedItem.FileContentModel.VideoMedia.StopForce();
                        }
                        fullScreen.Show();
                        this.Close();
                        e.Handled = true;
                        return;
                    }
                }
            }
            catch
            {
            }
        }

        private void GridFileContent_KeyDown(object sender, KeyEventArgs e)
        {
            HandleNavigationAndKeys(sender, e, true);
        }

        private void FilesListView_KeyDown(object sender, KeyEventArgs e)
        {
            HandleNavigationAndKeys(sender, e, false);
        }

        private void HandleNavigationAndKeys(object sender, KeyEventArgs e, bool fromContentGrid)
        {
            if (DataContext is FilesListViewModel vm)
            {
                bool isTextBoxFocused = System.Windows.Input.Keyboard.FocusedElement is TextBox;
                bool isTextFileOpen = vm.SelectedItem?.FileContentModel?.ShowTextBox == true;

                if (isTextFileOpen && isTextBoxFocused && fromContentGrid)
                {
                    return;
                }

                if (vm.SelectedItem?.FileContentModel?.ShowTextBox == true && isTextFileOpen && isTextBoxFocused)
                {
                    if (e.Key == Key.Add)
                    {
                        TextBoxTextContent.FontSize += 0.5;
                        ConfigHelper.SetFontSize(TextBoxTextContent.FontSize);
                        e.Handled = true;
                        return;
                    }
                    else if (e.Key == Key.Subtract)
                    {
                        if (TextBoxTextContent.FontSize >= 1)
                        {
                            TextBoxTextContent.FontSize -= 0.5;
                            ConfigHelper.SetFontSize(TextBoxTextContent.FontSize);
                        }
                        e.Handled = true;
                        return;
                    }
                }

                if (vm.SelectedItem?.FileContentModel.TextContent is not null && isTextFileOpen)
                {
                    if (e.Key == Key.Escape)
                    {
                        SearchTextBox.Text = string.Empty;
                        SearchResultsCount.Text = string.Empty;
                        _searchResults.Clear();
                        _currentSearchIndex = -1;
                        e.Handled = true;
                        return;
                    }
                }

                if (!isTextBoxFocused || !fromContentGrid)
                {
                    if (e.Key == Key.Multiply)
                    {
                        degreesRotation += 90;
                        if (degreesRotation < 360)
                        {
                            GridFileContent.LayoutTransform = new RotateTransform(degreesRotation);
                        }
                        else
                        {
                            degreesRotation = 0;
                            GridFileContent.LayoutTransform = new RotateTransform(0);
                        }
                        e.Handled = true;
                        return;
                    }

                    if (e.Key >= Key.A && e.Key <= Key.Z)
                    {
                        char ASCIINumberWhichUserWantToSelect = e.Key.ToString()[0];
                        var itemToSelect = FilesListView.Items.Cast<QuickViewFile.Models.ItemList>().Skip(FilesListView.SelectedIndex + 1)
                            .FirstOrDefault(item => !string.IsNullOrEmpty(item.Name) && char.ToUpper(item.Name[0]) == ASCIINumberWhichUserWantToSelect);

                        if (itemToSelect is null)
                        {
                            itemToSelect = FilesListView.Items.Cast<QuickViewFile.Models.ItemList>()
                                .FirstOrDefault(item => !string.IsNullOrEmpty(item.Name) && char.ToUpper(item.Name[0]) == ASCIINumberWhichUserWantToSelect);
                        }
                        if (itemToSelect is not null)
                        {
                            FilesListView.SelectedItem = itemToSelect;
                            FilesListView.ScrollIntoView(itemToSelect);
                        }
                        e.Handled = true;
                        return;
                    }
                }

                if (!isTextBoxFocused)
                {
                    try
                    {
                        bool handled = false;
                        if (e.Key == Key.Right || e.Key == Key.Down)
                        {
                            FilesListView.SelectedIndex++;
                            handled = true;
                        }
                        else if (e.Key == Key.Left || e.Key == Key.Up)
                        {
                            FilesListView.SelectedIndex--;
                            handled = true;
                        }
                        else if (e.Key == Key.PageDown)
                        {
                            FilesListView.SelectedIndex = Math.Min(FilesListView.Items.Count - 1, FilesListView.SelectedIndex + 10);
                            handled = true;
                        }
                        else if (e.Key == Key.PageUp)
                        {
                            FilesListView.SelectedIndex = Math.Max(0, FilesListView.SelectedIndex - 10);
                            handled = true;
                        }
                        else if (e.Key == Key.Home)
                        {
                            FilesListView.SelectedIndex = 0;
                            handled = true;
                        }
                        else if (e.Key == Key.End)
                        {
                            FilesListView.SelectedIndex = FilesListView.Items.Count - 1;
                            handled = true;
                        }

                        if (handled)
                        {
                            if (FilesListView.SelectedIndex < 0) FilesListView.SelectedIndex = 0;
                            if (FilesListView.SelectedIndex >= FilesListView.Items.Count) FilesListView.SelectedIndex = FilesListView.Items.Count - 1;

                            FilesListView.SetCurrentValue(ListView.SelectedIndexProperty, FilesListView.SelectedIndex);
                            FilesListView.ScrollIntoView(FilesListView.SelectedItem);
                            e.Handled = true;
                            return;
                        }
                    }
                    catch { }
                }

                if (e.Key == Key.Enter)
                {
                    Application.Current.Dispatcher.BeginInvoke(async () =>
                    {
                        await vm.OnFileDoubleClick(vm.SelectedItem);
                    });
                    e.Handled = true;
                }
            }
        }

        private void StatusBarTextBlock_MouseDown'''

text = re.sub(appwindow_pattern, replacement, text)

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
