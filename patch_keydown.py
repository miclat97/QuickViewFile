import re

with open('QuickViewFile/MainWindow.xaml.cs', 'r') as f:
    text = f.read()

# Modify AppWindow_KeyDown to handle textbox focus ignore, removing global logic and applying it correctly.

keydown_old = r'''        private void AppWindow_KeyDown\(object sender, KeyEventArgs e\)\s*\{\s*try\s*\{\s*if \(e\.Key == Key\.F11\)\s*\{\s*if \(_filesListViewVisible\)\s*\{\s*HideUI\(\);\s*\}\s*else\s*\{\s*ShowUI\(\);\s*\}\s*\}\s*if \(DataContext is FilesListViewModel vm\)\s*\{\s*try\s*\{\s*if \(e\.Key == Key\.Right \|\| e\.Key == Key\.Down\)\s*\{\s*FilesListView\.SelectedIndex\+\+;\s*\}\s*else if \(e\.Key == Key\.Left \|\| e\.Key == Key\.Up\)\s*\{\s*FilesListView\.SelectedIndex--;\s*\}\s*else if \(e\.Key == Key\.PageDown\)\s*\{\s*if \(FilesListView\.SelectedIndex \+ 10 >= FilesListView\.Items\.Count\)\s*\{\s*FilesListView\.SelectedIndex = FilesListView\.Items\.Count - 1;\s*\}\s*else\s*\{\s*FilesListView\.SelectedIndex \+= 10;\s*\}\s*\}\s*else if \(e\.Key == Key\.PageUp\)\s*\{\s*if \(FilesListView\.SelectedIndex - 10 < 0\)\s*\{\s*FilesListView\.SelectedIndex = 0;\s*\}\s*else\s*\{\s*FilesListView\.SelectedIndex -= 10;\s*\}\s*\}\s*if \(e\.Key == Key\.Home\)\s*\{\s*FilesListView\.SelectedIndex = 0;\s*\}\s*else if \(e\.Key == Key\.End\)\s*\{\s*FilesListView\.SelectedIndex = FilesListView\.Items\.Count - 1;\s*\}\s*if \(FilesListView\.SelectedIndex < 0\)\s*\{\s*FilesListView\.SelectedIndex = 0;\s*\}\s*else if \(FilesListView\.SelectedIndex >= FilesListView\.Items\.Count\)\s*\{\s*FilesListView\.SelectedIndex = FilesListView\.Items\.Count - 1;\s*\}\s*FilesListView\.SetCurrentValue\(ListView\.SelectedIndexProperty, FilesListView\.SelectedIndex\);\s*FilesListView\.ScrollIntoView\(FilesListView\.SelectedItem\);\s*\}\s*catch \(Exception\)\s*\{\s*\}\s*if \(vm\.SelectedItem\?\.FileContentModel\?\.ShowTextBox == true\)\s*\{\s*if \(e\.Key == Key\.Add\)\s*\{\s*TextBoxTextContent\.FontSize \+= 0\.5;\s*\}\s*else if \(e\.Key == Key\.Subtract\)\s*\{\s*if \(TextBoxTextContent\.FontSize >= 1\)\s*\{\s*TextBoxTextContent\.FontSize -= 0\.5;\s*\}\s*\}\s*ConfigHelper\.SetFontSize\(TextBoxTextContent\.FontSize\);\s*\}\s*if \(e\.Key == Key\.F4 && vm\.SelectedItem\?\.FullPath is not null\)\s*\{\s*MainWindowNoBorder fullScreen = new MainWindowNoBorder\(vm\.SelectedItem\.FullPath\);\s*if \(vm\.SelectedItem\.FileContentModel\.VideoMedia is not null\)\s*\{\s*vm\.SelectedItem\.FileContentModel\.VideoMedia\.StopForce\(\);\s*\}\s*fullScreen\.Show\(\);\s*this\.Close\(\);\s*\}\s*if \(vm\.SelectedItem\?\.FileContentModel\.TextContent is not null\)\s*\{\s*if \(e\.Key == Key\.Escape\)\s*\{\s*SearchTextBox\.Text = string\.Empty;\s*SearchResultsCount\.Text = string\.Empty;\s*_searchResults\.Clear\(\);\s*_currentSearchIndex = -1;\s*e\.Handled = true;\s*\}\s*\}\s*if \(e\.Key == Key\.Multiply\)\s*\{\s*degreesRotation \+= 90;\s*if \(degreesRotation < 360\)\s*\{\s*GridFileContent\.LayoutTransform = new RotateTransform\(degreesRotation\);\s*\}\s*else\s*\{\s*degreesRotation = 0;\s*GridFileContent\.LayoutTransform = new RotateTransform\(0\);\s*\}\s*\}\s*if \(e\.Key >= Key\.A && e\.Key <= Key\.Z\)\s*\{\s*char ASCIINumberWhichUserWantToSelect = e\.Key\.ToString\(\)\[0\];\s*var nextItems = new ItemList\(\);\s*var itemToSelect = FilesListView\.Items\.Cast<QuickViewFile\.Models\.ItemList>\(\)\.Skip\(FilesListView\.SelectedIndex \+ 1\)\s*\.FirstOrDefault\(item => !string\.IsNullOrEmpty\(item\.Name\) && char\.ToUpper\(item\.Name\[0\]\) == ASCIINumberWhichUserWantToSelect\);\s*if \(itemToSelect is null\)\s*\{\s*itemToSelect = FilesListView\.Items\.Cast<QuickViewFile\.Models\.ItemList>\(\)\s*\.FirstOrDefault\(item => !string\.IsNullOrEmpty\(item\.Name\) && char\.ToUpper\(item\.Name\[0\]\) == ASCIINumberWhichUserWantToSelect\);\s*\}\s*if \(itemToSelect is not null\)\s*\{\s*FilesListView\.SelectedItem = itemToSelect;\s*FilesListView\.ScrollIntoView\(itemToSelect\);\s*\}\s*\}\s*if \(e\.Key == Key\.Enter\)\s*\{\s*Application\.Current\.Dispatcher\.BeginInvoke\(async \(\) =>\s*\{\s*await vm\.OnFileDoubleClick\(vm\.SelectedItem\);\s*\}\);\s*\}\s*\}\s*\}\s*catch\s*\{\s*\}\s*\}'''

text = re.sub(keydown_old, '''
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
                        MainWindowNoBorder fullScreen = new MainWindowNoBorder(vm.SelectedItem.FullPath);
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
''', text)


text = text.replace(
'''        private void FilesListView_KeyDown(object sender, KeyEventArgs e)
        {

        }''',
'''        private void GridFileContent_KeyDown(object sender, KeyEventArgs e)
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
                // Prevent intercepting keys if the user is typing in a TextBox (e.g. search box or text preview),
                // EXCEPT if the focus is on the List and we are navigating (fromContentGrid is false).
                bool isTextBoxFocused = System.Windows.Input.Keyboard.FocusedElement is TextBox;
                bool isTextFileOpen = vm.SelectedItem?.FileContentModel?.ShowTextBox == true;

                if (isTextFileOpen && isTextBoxFocused && fromContentGrid)
                {
                    // Allow normal text box typing
                    return;
                }

                if (vm.SelectedItem?.FileContentModel?.ShowTextBox == true && isTextFileOpen && isTextBoxFocused)
                {
                    // Handle zoom shortcuts when viewing text
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
        }''')

with open('QuickViewFile/MainWindow.xaml.cs', 'w') as f:
    f.write(text)
