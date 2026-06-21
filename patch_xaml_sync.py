import re

def fix_xaml_sync(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    text = text.replace('KeyDown="FilesListView_KeyDown"', 'KeyDown="FilesListView_KeyDown"\n                  SelectionChanged="FilesListView_SelectionChanged"')

    with open(file_path, 'w') as f:
        f.write(text)

fix_xaml_sync('QuickViewFile/MainWindow.xaml')
fix_xaml_sync('QuickViewFile/MainWindowNoBorder.xaml')
