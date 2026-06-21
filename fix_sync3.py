with open('QuickViewFile/MainWindow.xaml', 'r') as f:
    text = f.read()

text = text.replace('SelectionMode="Extended"', 'SelectionMode="Extended"\n                  SelectionChanged="FilesListView_SelectionChanged"')

with open('QuickViewFile/MainWindow.xaml', 'w') as f:
    f.write(text)

with open('QuickViewFile/MainWindowNoBorder.xaml', 'r') as f:
    text = f.read()

text = text.replace('SelectionMode="Extended"', 'SelectionMode="Extended"\n                  SelectionChanged="FilesListView_SelectionChanged"')

with open('QuickViewFile/MainWindowNoBorder.xaml', 'w') as f:
    f.write(text)


with open('QuickViewFile/MainWindow.xaml.cs', 'r') as f:
    text = f.read()

if 'FilesListView_SelectionChanged' not in text:
    text = text.replace('private void FilesListView_KeyDown', '''private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && FilesListView.SelectedItem is ItemList selected)
            {
                vm.SelectedItem = selected;
            }
        }\n\n        private void FilesListView_KeyDown''')

with open('QuickViewFile/MainWindow.xaml.cs', 'w') as f:
    f.write(text)


with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

if 'FilesListView_SelectionChanged' not in text:
    text = text.replace('private void FilesListView_KeyDown', '''private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && FilesListView.SelectedItem is ItemList selected)
            {
                vm.SelectedItem = selected;
            }
        }\n\n        private void FilesListView_KeyDown''')

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
