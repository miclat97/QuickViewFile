with open('QuickViewFile/SettingsWindow.xaml.cs', 'r') as f:
    text = f.read()

restore_code = '''
        private void RestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            _config = new ConfigModel();
            SettingsPanel.Children.Clear();
            GenerateUI();
        }
'''

text = text.replace('private void SaveButton_Click(object sender, RoutedEventArgs e)', restore_code + '        private void SaveButton_Click(object sender, RoutedEventArgs e)')

with open('QuickViewFile/SettingsWindow.xaml.cs', 'w') as f:
    f.write(text)
