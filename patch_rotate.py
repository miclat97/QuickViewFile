import re

def add_rotate_btn(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    btn = '''<Button Content="Rotate" Name="RotateButton" Height="24" Margin="0,8,10,8" Padding="10,0" Click="RotateButton_Click" ToolTip="TIP: * can do this"/>
                <Button Content="Full Screen"'''
    text = text.replace('<Button Content="Full Screen"', btn)
    text = text.replace('<Button Content="Exit Full Screen"', btn.replace('Full Screen', 'Exit Full Screen'))
    with open(file_path, 'w') as f:
        f.write(text)

add_rotate_btn('QuickViewFile/MainWindow.xaml')
add_rotate_btn('QuickViewFile/MainWindowNoBorder.xaml')
