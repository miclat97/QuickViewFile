with open('QuickViewFile/MainWindow.xaml.cs', 'r') as f:
    text = f.read()

# Fix SettingsWindow missing using
if 'using QuickViewFile;' not in text:
    text = 'using QuickViewFile;\n' + text
with open('QuickViewFile/MainWindow.xaml.cs', 'w') as f:
    f.write(text)

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()
if 'using QuickViewFile;' not in text:
    text = 'using QuickViewFile;\n' + text
with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
