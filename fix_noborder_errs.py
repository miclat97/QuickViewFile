import re

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

text = text.replace('private void ExitFullScreenButton_Click(object sender, RoutedEventArgs e)', 'public void ExitFullScreenButton_Click(object sender, RoutedEventArgs e)')

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
