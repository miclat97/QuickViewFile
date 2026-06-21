import re

def fix_cs(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    paste_logic_search = r'''                if \(_clipboardFiles.Count > 0\)
                \{
                    PasteButton.Visibility = Visibility.Visible;
                    PasteButton.Content = \$\"Paste \(\{_clipboardFiles.Count\} files\)\";
                \}'''

    paste_logic_replace = r'''                if (_clipboardFiles.Count > 0)
                {
                    MoveButton.Visibility = Visibility.Collapsed;
                    CopyButton.Visibility = Visibility.Collapsed;
                    DeleteButton.Visibility = Visibility.Collapsed;
                    PasteButton.Visibility = Visibility.Visible;
                    PasteButton.Content = $"Paste ({_clipboardFiles.Count} files)";
                }'''

    text = re.sub(paste_logic_search, paste_logic_replace, text)


    paste_clear_search = r'''                if \(_currentOperation == FileOperation.Move\)
                \{
                    _clipboardFiles.Clear\(\);
                    _currentOperation = FileOperation.None;
                    PasteButton.Visibility = Visibility.Collapsed;
                \}'''

    paste_clear_replace = r'''                if (_currentOperation == FileOperation.Move || _currentOperation == FileOperation.Copy)
                {
                    _clipboardFiles.Clear();
                    _currentOperation = FileOperation.None;
                    PasteButton.Visibility = Visibility.Collapsed;
                    MoveButton.Visibility = Visibility.Visible;
                    CopyButton.Visibility = Visibility.Visible;
                    DeleteButton.Visibility = Visibility.Visible;
                }'''

    text = re.sub(paste_clear_search, paste_clear_replace, text)

    # Sorting Freeze Fix: Use a new ObservableCollection instance instead of iterating
    sort_search = r'''                vm.ActiveListItems.Clear\(\);
                if \(parentDir != null\)
                    vm.ActiveListItems.Add\(parentDir\);

                foreach \(var folder in folders\)
                    vm.ActiveListItems.Add\(folder\);

                foreach \(var file in files\)
                    vm.ActiveListItems.Add\(file\);'''

    sort_replace = r'''                var newList = new System.Collections.ObjectModel.ObservableCollection<ItemList>();
                if (parentDir != null) newList.Add(parentDir);
                foreach (var folder in folders) newList.Add(folder);
                foreach (var file in files) newList.Add(file);
                vm.ActiveListItems = newList;
                FilesListView.ItemsSource = vm.ActiveListItems;'''

    text = re.sub(sort_search, sort_replace, text)

    # Text Box Focus Fix: Only navigate if fromContentGrid is False
    nav_search = r'''                if \(isTextFileOpen && isTextBoxFocused && fromContentGrid\)
                \{
                    return;
                \}'''

    nav_replace = r'''                if (isTextFileOpen && fromContentGrid)
                {
                    return;
                }'''

    text = re.sub(nav_search, nav_replace, text)

    # Fix selection sync
    select_fix_search = r'''        private void FilesListView_KeyDown\(object sender, KeyEventArgs e\)
        \{
            HandleNavigationAndKeys\(sender, e, false\);
        \}'''

    select_fix_replace = r'''        private void FilesListView_KeyDown(object sender, KeyEventArgs e)
        {
            HandleNavigationAndKeys(sender, e, false);
        }

        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && FilesListView.SelectedItem is ItemList selected)
            {
                vm.SelectedItem = selected;
            }
        }'''

    if 'FilesListView_SelectionChanged' not in text:
        text = re.sub(select_fix_search, select_fix_replace, text)

    with open(file_path, 'w') as f:
        f.write(text)

fix_cs('QuickViewFile/MainWindow.xaml.cs')
fix_cs('QuickViewFile/MainWindowNoBorder.xaml.cs')
