import sys

def replace_in_file(filepath, search_str, replace_str):
    with open(filepath, 'r') as f:
        content = f.read()

    if search_str in content:
        content = content.replace(search_str, replace_str)
        with open(filepath, 'w') as f:
            f.write(content)
        print(f"Patched {filepath}")
    else:
        print(f"Could not find search string in {filepath}")

cs_search = """                    if (e.Key == Key.Space)
                    {
                        var focusedElement = System.Windows.Input.Keyboard.FocusedElement as ListViewItem;
                        if (focusedElement == null)
                        {
                            // Fallback to selected item if focus is lost but something is selected
                            if (FilesListView.SelectedItem != null)
                            {
                                focusedElement = FilesListView.ItemContainerGenerator.ContainerFromItem(FilesListView.SelectedItem) as ListViewItem;
                            }
                        }

                        if (focusedElement != null)
                        {
                            // Toggle selection without clearing the rest
                            focusedElement.IsSelected = !focusedElement.IsSelected;

                            int currentIndex = FilesListView.ItemContainerGenerator.IndexFromContainer(focusedElement);
                            int nextIndex = currentIndex + 1;

                            if (nextIndex < FilesListView.Items.Count)
                            {
                                var nextItem = FilesListView.Items[nextIndex];
                                FilesListView.ScrollIntoView(nextItem);

                                // Delay focus slightly to ensure the container is generated
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    var nextContainer = FilesListView.ItemContainerGenerator.ContainerFromIndex(nextIndex) as ListViewItem;
                                    if (nextContainer != null)
                                    {
                                        nextContainer.Focus();
                                    }
                                }), System.Windows.Threading.DispatcherPriority.Background);
                            }
                        }
                        e.Handled = true;
                        return;
                    }"""

cs_replace = """                    if (e.Key == Key.Space)
                    {
                        var focusedElement = System.Windows.Input.Keyboard.FocusedElement as ListViewItem;
                        if (focusedElement == null)
                        {
                            // Fallback to selected item if focus is lost but something is selected
                            if (FilesListView.SelectedItem != null)
                            {
                                focusedElement = FilesListView.ItemContainerGenerator.ContainerFromItem(FilesListView.SelectedItem) as ListViewItem;
                            }
                        }

                        if (focusedElement != null)
                        {
                            var itemData = FilesListView.ItemContainerGenerator.ItemFromContainer(focusedElement) as QuickViewFile.Models.ItemList;
                            if (itemData != null)
                            {
                                itemData.IsChecked = !itemData.IsChecked;

                                int currentIndex = FilesListView.ItemContainerGenerator.IndexFromContainer(focusedElement);
                                int nextIndex = currentIndex + 1;

                                if (nextIndex < FilesListView.Items.Count)
                                {
                                    // Move selection to the next item
                                    FilesListView.SelectedIndex = nextIndex;
                                    FilesListView.ScrollIntoView(FilesListView.SelectedItem);

                                    // Make sure it also gets focus so next spacebar works
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        var nextContainer = FilesListView.ItemContainerGenerator.ContainerFromIndex(nextIndex) as ListViewItem;
                                        if (nextContainer != null)
                                        {
                                            nextContainer.Focus();
                                        }
                                    }), System.Windows.Threading.DispatcherPriority.Background);
                                }
                            }
                        }
                        e.Handled = true;
                        return;
                    }"""

replace_in_file("QuickViewFile/MainWindow.xaml.cs", cs_search, cs_replace)
replace_in_file("QuickViewFile/MainWindowNoBorder.xaml.cs", cs_search, cs_replace)
