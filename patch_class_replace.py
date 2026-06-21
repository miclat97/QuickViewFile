import re

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

# Replace any lingering MainWindow references to MainWindowNoBorder (like constructors)
text = re.sub(r'public MainWindow\(', 'public MainWindowNoBorder(', text)

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
