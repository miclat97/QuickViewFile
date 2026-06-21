import re

with open('QuickViewFile/MainWindow.xaml', 'r') as f:
    text = f.read()

text = text.replace(
'''<Grid MouseRightButtonDown="FileContentGrid_MouseRightButtonDown"
                          Grid.Row="0"
                          Name="GridFileContent" ClipToBounds="True" Grid.IsSharedSizeScope="True" IsManipulationEnabled="True" SnapsToDevicePixels="True">''',
'''<Grid MouseRightButtonDown="FileContentGrid_MouseRightButtonDown"
                          Grid.Row="0"
                          Name="GridFileContent" ClipToBounds="True" Grid.IsSharedSizeScope="True" IsManipulationEnabled="True" SnapsToDevicePixels="True"
                          KeyDown="GridFileContent_KeyDown" Focusable="True">''')

# Also remove KeyDown from Window tag
text = re.sub(r'KeyDown="AppWindow_KeyDown"\n\s*', '', text)

with open('QuickViewFile/MainWindow.xaml', 'w') as f:
    f.write(text)
