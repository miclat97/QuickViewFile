def add_rotate_cs(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    rotate_code = '''
        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            degreesRotation += 90;
            if (degreesRotation < 360)
            {
                GridFileContent.LayoutTransform = new System.Windows.Media.RotateTransform(degreesRotation);
            }
            else
            {
                degreesRotation = 0;
                GridFileContent.LayoutTransform = new System.Windows.Media.RotateTransform(0);
            }
        }
'''

    text = text.replace('private void SettingsButton_Click(object sender, RoutedEventArgs e)', rotate_code + '        private void SettingsButton_Click(object sender, RoutedEventArgs e)')

    with open(file_path, 'w') as f:
        f.write(text)

add_rotate_cs('QuickViewFile/MainWindow.xaml.cs')
add_rotate_cs('QuickViewFile/MainWindowNoBorder.xaml.cs')
