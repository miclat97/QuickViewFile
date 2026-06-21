import re

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

bg_code = '''
                if (_config.TransparentBackgroundInFullScreenMode == 0)
                {
                    MainGrid.Background = new SolidColorBrush(Color.FromRgb(17, 17, 17));
                }
                else
                {
                    MainGrid.Background = Brushes.Transparent;
                }'''

text = text.replace('FilesListView.Focus();', 'FilesListView.Focus();\n' + bg_code)

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
