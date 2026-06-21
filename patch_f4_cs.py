with open('QuickViewFile/MainWindow.xaml.cs', 'r') as f:
    text = f.read()

f4_code = '''
        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && vm.SelectedItem?.FullPath is not null)
            {
                MainWindowNoBorder fullScreen = new MainWindowNoBorder(vm.SelectedItem.FullPath);
                if (vm.SelectedItem.FileContentModel.VideoMedia is not null)
                {
                    vm.SelectedItem.FileContentModel.VideoMedia.StopForce();
                }
                fullScreen.Show();
                this.Close();
            }
        }
'''

text = text.replace('private void SettingsButton_Click', f4_code + '        private void SettingsButton_Click')

with open('QuickViewFile/MainWindow.xaml.cs', 'w') as f:
    f.write(text)
