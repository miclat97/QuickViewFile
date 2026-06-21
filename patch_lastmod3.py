with open('QuickViewFile/MainWindow.xaml.cs', 'r') as f:
    text = f.read()

# remove SelectionChanged
if 'private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)' in text:
    import re
    text = re.sub(r'private void FilesListView_SelectionChanged[\s\S]*?\}\s*\}', '', text)

with open('QuickViewFile/MainWindow.xaml.cs', 'w') as f:
    f.write(text)

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

if 'private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)' in text:
    import re
    text = re.sub(r'private void FilesListView_SelectionChanged[\s\S]*?\}\s*\}', '', text)

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
