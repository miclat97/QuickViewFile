import re

def fix_col_click(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        text = f.read()

    text = text.replace('var headerClicked = e.OriginalSource as GridViewColumnHeader;', 'var headerClicked = sender as GridViewColumnHeader;')

    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(text)

fix_col_click('QuickViewFile/MainWindow.xaml.cs')
fix_col_click('QuickViewFile/MainWindowNoBorder.xaml.cs')
