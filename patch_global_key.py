import re

def fix_global_key(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    # Revert to original AppWindow_KeyDown with modified guard logic
    # Find the old HandleNavigationAndKeys block and remove it entirely.
    search = r'private void GridFileContent_KeyDown\(object sender, KeyEventArgs e\)[\s\S]*?private void StatusBarTextBlock_MouseDown'

    replace = r'''private void AppWindow_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.F11)
                {
                    if (_filesListViewVisible) HideUI();
                    else ShowUI();
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

                    bool isTextBoxFocused = System.Windows.Input.Keyboard.FocusedElement is TextBox;
                    bool isTextFileOpen = vm.SelectedItem?.FileContentModel?.ShowTextBox == true;

                    // If a text file is open, and focus is OUTSIDE the list (i.e. we are typing in text box),
                    // Do NOT intercept navigation keys!
                    if (isTextFileOpen && isTextBoxFocused)
                    {
                        if (e.Key == Key.Add)
                        {
                            TextBoxTextContent.FontSize += 0.5;
                            ConfigHelper.SetFontSize(TextBoxTextContent.FontSize);
                            e.Handled = true;
                        }
                        else if (e.Key == Key.Subtract)
                        {
                            if (TextBoxTextContent.FontSize >= 1)
                            {
                                TextBoxTextContent.FontSize -= 0.5;
                                ConfigHelper.SetFontSize(TextBoxTextContent.FontSize);
                            }
                            e.Handled = true;
                        }

                        // We return here so we don't accidentally navigate files while typing!
                        return;
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

                    // Proceed with Global File Navigation
                    if (e.Key == Key.Multiply)
                    {
                        degreesRotation += 90;
                        if (degreesRotation < 360) GridFileContent.LayoutTransform = new RotateTransform(degreesRotation);
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
            catch { }
        }

        private void FilesListView_KeyDown(object sender, KeyEventArgs e)
        {
            // Navigation handled globally
        }

        private void StatusBarTextBlock_MouseDown'''

    # First, let's remove the old AppWindow_KeyDown implementation entirely,
    # to avoid duplicates before injecting the single clean one.

    # We'll just replace the original old AppWindow_KeyDown that was left if we left it earlier.
    # Ah, I replaced AppWindow_KeyDown in my previous script and made it only have F11 and F4.

    # So let's delete the small AppWindow_KeyDown, and replace HandleNavigationAndKeys etc.
    text = re.sub(r'private void AppWindow_KeyDown\(object sender, KeyEventArgs e\)[\s\S]*?private void GridFileContent_KeyDown', 'private void GridFileContent_KeyDown', text)

    text = re.sub(search, replace, text)

    if 'MainWindowNoBorder' in text:
        text = text.replace('MainWindowNoBorder fullScreen = new MainWindowNoBorder(vm.SelectedItem.FullPath);', 'MainWindow fullScreen = new MainWindow(vm.SelectedItem.FullPath);')

    with open(file_path, 'w') as f:
        f.write(text)

fix_global_key('QuickViewFile/MainWindow.xaml.cs')
fix_global_key('QuickViewFile/MainWindowNoBorder.xaml.cs')
