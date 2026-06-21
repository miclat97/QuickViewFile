import re

def re_add_missing(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    missing_funcs = '''
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            if (settingsWindow.ShowDialog() == true)
            {
                if (DataContext is FilesListViewModel vm)
                {
                    vm.Config = ConfigHelper.LoadConfig();
                }
            }
        }
'''

    if 'SettingsButton_Click' not in text:
        text = text.replace('        private enum FileOperation', missing_funcs + '        private enum FileOperation')

    if 'MainWindow.xaml.cs' in file_path and 'FullScreenButton_Click' not in text:
        f4_btn = '''
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
        text = text.replace('        private void SettingsButton_Click', f4_btn + '        private void SettingsButton_Click')

    if 'MainWindowNoBorder.xaml.cs' in file_path and 'ExitFullScreenButton_Click' not in text:
        f4_btn = '''
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
        text = text.replace('        private void SettingsButton_Click', f4_btn + '        private void SettingsButton_Click')


    with open(file_path, 'w') as f:
        f.write(text)

re_add_missing('QuickViewFile/MainWindow.xaml.cs')
re_add_missing('QuickViewFile/MainWindowNoBorder.xaml.cs')
