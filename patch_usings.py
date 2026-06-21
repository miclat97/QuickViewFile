import re

def fix_usings(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    text = text.replace('private ListSortDirection', 'private System.ComponentModel.ListSortDirection')
    text = text.replace('ListSortDirection direction;', 'System.ComponentModel.ListSortDirection direction;')
    text = text.replace('ListSortDirection direction)', 'System.ComponentModel.ListSortDirection direction)')
    text = text.replace('ListSortDirection.Ascending', 'System.ComponentModel.ListSortDirection.Ascending')
    text = text.replace('ListSortDirection.Descending', 'System.ComponentModel.ListSortDirection.Descending')

    with open(file_path, 'w') as f:
        f.write(text)

fix_usings('QuickViewFile/MainWindow.xaml.cs')
fix_usings('QuickViewFile/MainWindowNoBorder.xaml.cs')
