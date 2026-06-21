import re

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

text = text.replace('public partial class MainWindow : Window', 'public partial class MainWindowNoBorder : Window')

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
