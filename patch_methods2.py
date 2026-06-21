import re

def re_add_missing(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    missing_funcs = '''
        private void FilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && FilesListView.SelectedItem is ItemList selected)
            {
                vm.SelectedItem = selected;
            }
        }
'''

    if 'FilesListView_SelectionChanged' not in text:
        text = text.replace('        private void FilesListView_KeyDown', missing_funcs + '        private void FilesListView_KeyDown')

    with open(file_path, 'w') as f:
        f.write(text)

re_add_missing('QuickViewFile/MainWindow.xaml.cs')
re_add_missing('QuickViewFile/MainWindowNoBorder.xaml.cs')
