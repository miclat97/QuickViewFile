import re

with open('QuickViewFile/MainWindow.xaml', 'r') as f:
    text = f.read()

# Add Focus mechanism fixes
text = text.replace('Focusable="True"\n                  IsTabStop="True"', 'Focusable="False"\n                  IsTabStop="False"')
text = text.replace('SelectionChanged="FilesListView_SelectionChanged"', '')

with open('QuickViewFile/MainWindow.xaml', 'w') as f:
    f.write(text)

with open('QuickViewFile/MainWindowNoBorder.xaml', 'r') as f:
    text = f.read()

text = text.replace('Focusable="True"\n                  IsTabStop="True"', 'Focusable="False"\n                  IsTabStop="False"')
text = text.replace('SelectionChanged="FilesListView_SelectionChanged"', '')

with open('QuickViewFile/MainWindowNoBorder.xaml', 'w') as f:
    f.write(text)
