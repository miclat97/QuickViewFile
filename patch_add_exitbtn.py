with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

exit_btn = '''
        private void ExitFullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is FilesListViewModel vm && vm.SelectedItem?.FullPath is not null)
            {
                MainWindow fullScreen = new MainWindow(vm.SelectedItem.FullPath);
                if (vm.SelectedItem.FileContentModel.VideoMedia is not null)
                {
                    vm.SelectedItem.FileContentModel.VideoMedia.StopForce();
                }
                fullScreen.Show();
                this.Close();
            }
        }
'''

text = text.replace('private void SettingsButton_Click(object sender, RoutedEventArgs e)', exit_btn + '        private void SettingsButton_Click(object sender, RoutedEventArgs e)')

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
