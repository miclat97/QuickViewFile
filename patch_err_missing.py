import re

def re_add_missing(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    missing_funcs = '''
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DataContext is FilesListViewModel vm)
                {
                    Point mousePosition = e.GetPosition(GridFileContent);

                    double previousItem = GridFileContent.ActualWidth * 0.08;
                    double nextItem = GridFileContent.ActualWidth * 0.92;

                    int nextFileIndex = FilesListView.SelectedIndex + 1;
                    int previousFileIndex = FilesListView.SelectedIndex - 1;

                    if (mousePosition.X < previousItem)
                    {
                        FilesListView.SelectedIndex--;
                        System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.None);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                        System.Windows.Input.Mouse.UpdateCursor();
                    }

                    if (mousePosition.X > nextItem)
                    {
                        FilesListView.SelectedIndex++;
                        System.Windows.Input.Mouse.SetCursor(System.Windows.Input.Cursors.None);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                        System.Windows.Input.Mouse.UpdateCursor();
                    }

                    if (FilesListView.SelectedIndex < 0)
                        FilesListView.SelectedIndex = 0;
                    if (FilesListView.SelectedIndex >= FilesListView.Items.Count)
                        FilesListView.SelectedIndex = FilesListView.Items.Count - 1;

                    FilesListView.ScrollIntoView(FilesListView.SelectedItem);
                }
            }
            catch (Exception)
            {

            }
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void FileContentGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                try
                {
                    if (DataContext is FilesListViewModel vm)
                    {
                        if (_filesListViewVisible)
                        {
                            HideUI();
                        }
                        else
                        {
                            ShowUI();
                        }
                    }
                }
                catch
                {

                }
            }
        }

        private void HideUI()
        {
            _filesListColumnWidthCache = FilesListColumn.Width;

            FilesListColumn.Width = new GridLength(0);
            FilesListView.Visibility = Visibility.Collapsed;
            TopInfoPanel.Visibility = Visibility.Collapsed;
            MainWindowGridSplitter.Visibility = Visibility.Collapsed;
            StatusBar.Visibility = Visibility.Collapsed;
            _filesListViewVisible = false;
        }

        private void ShowUI()
        {
            FilesListColumn.Width = _filesListColumnWidthCache;

            TopInfoPanel.Visibility = Visibility.Visible;
            MainWindowGridSplitter.Visibility = Visibility.Visible;
            FilesListView.Visibility = Visibility.Visible;
            StatusBar.Visibility = Visibility.Visible;
            _filesListViewVisible = true;
            FilesListView.ScrollIntoView(FilesListView.SelectedItem);
            FilesListView.IsSynchronizedWithCurrentItem = true;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is FilesListViewModel vm &&
                    vm.SelectedItem?.FileContentModel?.ShowTextBox == true &&
                    !string.IsNullOrEmpty(vm.SelectedItem.FullPath))
                {
                    string content = TextBoxTextContent.Text;
                    string filePath = vm.SelectedItem.FullPath;

                    try
                    {
                        // Zapisz zawartość do pliku
                        await System.IO.File.WriteAllTextAsync(filePath, content, System.Text.Encoding.UTF8);

                        // Pokaż potwierdzenie
                        var originalBackground = SaveButton.Background;
                        SaveButton.Content = "Saved!";
                        await System.Threading.Tasks.Task.Delay(1500); // Pokazuj "Saved!" przez 1.5 sekundy
                        SaveButton.Content = "Save";
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(
                            "Access denied. The file may be read-only or you may not have required permissions.",
                            "Save Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    catch (System.IO.IOException ex)
                    {
                        MessageBox.Show(
                            $"Could not save the file: {ex.Message}",
                            "Save Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while saving: {ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private readonly List<int> _searchResults = new List<int>();
        private int _currentSearchIndex = -1;

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FindNext_Click(sender, null);
                e.Handled = true;
            }
        }

        private void FindNext_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text)) return;

            if (_searchResults.Count == 0 || TextBoxTextContent.Text.IndexOf(SearchTextBox.Text, StringComparison.OrdinalIgnoreCase) == -1)
            {
                // New search
                _searchResults.Clear();
                int index = 0;
                while ((index = TextBoxTextContent.Text.IndexOf(SearchTextBox.Text, index, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    _searchResults.Add(index);
                    index += SearchTextBox.Text.Length;
                }
                _currentSearchIndex = -1;
            }

            if (_searchResults.Count > 0)
            {
                _currentSearchIndex = (_currentSearchIndex + 1) % _searchResults.Count;
                HighlightSearchResult();
            }

            UpdateSearchCount();
        }

        private void FindPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (_searchResults.Count == 0 || string.IsNullOrEmpty(SearchTextBox.Text)) return;

            _currentSearchIndex--;
            if (_currentSearchIndex < 0)
                _currentSearchIndex = _searchResults.Count - 1;

            HighlightSearchResult();
            UpdateSearchCount();
        }

        private void HighlightSearchResult()
        {
            if (_currentSearchIndex >= 0 && _currentSearchIndex < _searchResults.Count)
            {
                TextBoxTextContent.Focus();
                TextBoxTextContent.Select(_searchResults[_currentSearchIndex], SearchTextBox.Text.Length);
                TextBoxTextContent.ScrollToLine(TextBoxTextContent.GetLineIndexFromCharacterIndex(_searchResults[_currentSearchIndex]));
            }
        }

        private void UpdateSearchCount()
        {
            if (_searchResults.Count > 0)
                SearchResultsCount.Text = $"{_currentSearchIndex + 1} of {_searchResults.Count} matches";
            else
                SearchResultsCount.Text = "No matches";
        }
'''

    text = text.replace('private void StatusBarTextBlock_MouseDown', missing_funcs + '        private void StatusBarTextBlock_MouseDown')

    with open(file_path, 'w') as f:
        f.write(text)

re_add_missing('QuickViewFile/MainWindow.xaml.cs')
re_add_missing('QuickViewFile/MainWindowNoBorder.xaml.cs')
