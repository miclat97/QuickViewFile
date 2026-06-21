with open('QuickViewFile/MainWindow.xaml', 'r') as f:
    text = f.read()

text = text.replace(
'''                <Button
                  Content="HQ"
                  Name="HQButton"
                  Height="24"
                  Margin="0,8,10,8"
                  Padding="10,0"
                  Click="HQButton_Click"
                  ToolTip="Toggle High Quality (Fant vs Linear)"/>''',
'''                <Button Content="Full Screen" Name="FullScreenButton" Height="24" Margin="0,8,10,8" Padding="10,0" Click="FullScreenButton_Click" ToolTip="TIP: F4 can do this"/>
                <Button Content="Settings" Name="SettingsButton" Height="24" Margin="0,8,10,8" Padding="10,0" Click="SettingsButton_Click" ToolTip="Open Settings"/>''')

with open('QuickViewFile/MainWindow.xaml', 'w') as f:
    f.write(text)
