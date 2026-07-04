import re

# 1. Update PasteLogic.cs
with open('QuickViewFile/PasteLogic.cs', 'r') as f:
    content = f.read()

# Replace the progress Callback to throttle Dispatcher invokes and use InvokeAsync
new_progress_callback = '''
                        int lastProgressPercentage = -1;
                        DateTime lastUpdateTime = DateTime.Now;

                        Action<long> progressCallback = (bytes) =>
                        {
                            copiedBytes += bytes;
                            if (totalBytes > 0)
                            {
                                int percentage = (int)((double)copiedBytes / totalBytes * 100);
                                if (percentage > lastProgressPercentage || (DateTime.Now - lastUpdateTime).TotalMilliseconds > 100)
                                {
                                    lastProgressPercentage = percentage;
                                    lastUpdateTime = DateTime.Now;
                                    Application.Current.Dispatcher.InvokeAsync(() =>
                                    {
                                        progressBar.Value = percentage;
                                    });
                                }
                            }
                        };
'''

content = re.sub(
    r'Action<long> progressCallback = \(bytes\) =>\s*\{\s*copiedBytes \+= bytes;\s*Application\.Current\.Dispatcher\.Invoke\(\(\) =>\s*\{\s*if \(totalBytes > 0\)\s*progressBar\.Value = \(double\)copiedBytes / totalBytes \* 100;\s*\}\);\s*\};',
    new_progress_callback,
    content,
    flags=re.DOTALL
)

# Also fix the other Dispatcher.Invokes in PasteLogic to use InvokeAsync where appropriate
content = content.replace('Application.Current.Dispatcher.Invoke(() => statusText.Text = $"Processing: {itemName}");', 'Application.Current.Dispatcher.InvokeAsync(() => statusText.Text = $"Processing: {itemName}");')

content = re.sub(
    r'Application\.Current\.Dispatcher\.Invoke\(\(\) =>\s*\{\s*if \(totalBytes > 0\)\s*progressBar\.Value = \(double\)copiedBytes / totalBytes \* 100;\s*\}\);',
    r'Application.Current.Dispatcher.InvokeAsync(() => { if (totalBytes > 0) progressBar.Value = (double)copiedBytes / totalBytes * 100; });',
    content
)

with open('QuickViewFile/PasteLogic.cs', 'w') as f:
    f.write(content)


# 2. Update FilesListViewModel.cs to notify FolderPath changed
with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'r') as f:
    vm_content = f.read()

# Add OnPropertyChanged(nameof(FolderPath)) to RefreshFiles
vm_content = vm_content.replace('StatusBarText = $"QuickViewFile v {appVersion}";', 'StatusBarText = $"QuickViewFile v {appVersion}";\n            OnPropertyChanged(nameof(FolderPath));')

with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'w') as f:
    f.write(vm_content)


# 3. Update XAMLs to bind TextBox to FolderPath and add PreviewMouseLeftButtonDown to ListViews
def update_xaml(filepath, listview_name):
    with open(filepath, 'r') as f:
        xaml = f.read()

    # Bind to FolderPath
    xaml = xaml.replace('Text="{Binding SelectedItem.FullPath, Mode=OneWay}"', 'Text="{Binding FolderPath, Mode=OneWay}"')

    # Add PreviewMouseLeftButtonDown
    old_lv = f'x:Name="{listview_name}"'
    new_lv = f'x:Name="{listview_name}"\n                  PreviewMouseLeftButtonDown="{listview_name}_PreviewMouseLeftButtonDown"'
    if new_lv not in xaml:
        xaml = xaml.replace(old_lv, new_lv)

    with open(filepath, 'w') as f:
        f.write(xaml)

update_xaml('QuickViewFile/MainWindow.xaml', 'FilesListView')
update_xaml('QuickViewFile/MainWindowNoBorder.xaml', 'FilesListView')
update_xaml('QuickViewFile/MainWindowThumbnails.xaml', 'ThumbnailsListView')


# 4. Update Code-Behinds with Shift-Click logic
shift_click_code = '''
        private int _lastCheckedIndex = -1;

        private void FilesListView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var dependencyObject = e.OriginalSource as System.Windows.DependencyObject;
            var listViewItem = FindVisualParent<System.Windows.Controls.ListViewItem>(dependencyObject);

            if (listViewItem != null && DataContext is QuickViewFile.ViewModel.FilesListViewModel vm)
            {
                var clickedData = listViewItem.DataContext as QuickViewFile.Models.ItemList;
                if (clickedData == null) return;

                int currentIndex = vm.ActiveListItems.IndexOf(clickedData);
                if (currentIndex == -1) return;

                if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift) && _lastCheckedIndex != -1)
                {
                    int start = Math.Min(_lastCheckedIndex, currentIndex);
                    int end = Math.Max(_lastCheckedIndex, currentIndex);

                    for (int i = start; i <= end; i++)
                    {
                        vm.ActiveListItems[i].IsChecked = true;
                    }
                    e.Handled = true;
                }
                else
                {
                    _lastCheckedIndex = currentIndex;
                }
            }
        }
'''

def append_to_class(path, text_to_append, is_thumbnails=False):
    with open(path, 'r') as f:
        content = f.read()

    if text_to_append in content:
        return

    code = text_to_append
    if is_thumbnails:
        code = code.replace('FilesListView_PreviewMouseLeftButtonDown', 'ThumbnailsListView_PreviewMouseLeftButtonDown')

    # Add FindVisualParent if not exists
    if 'FindVisualParent' not in content:
        code += '''
        public static T FindVisualParent<T>(System.Windows.DependencyObject child) where T : System.Windows.DependencyObject
        {
            System.Windows.DependencyObject parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }
'''

    last_brace = content.rfind('}')
    last_brace = content.rfind('}', 0, last_brace)
    new_content = content[:last_brace] + code + content[last_brace:]
    with open(path, 'w') as f:
        f.write(new_content)

append_to_class('QuickViewFile/MainWindow.xaml.cs', shift_click_code)
append_to_class('QuickViewFile/MainWindowNoBorder.xaml.cs', shift_click_code)
append_to_class('QuickViewFile/MainWindowThumbnails.xaml.cs', shift_click_code, is_thumbnails=True)
