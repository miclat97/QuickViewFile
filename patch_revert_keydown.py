import re

def revert_keydown(file_path):
    with open(file_path, 'r') as f:
        text = f.read()

    # Add KeyDown="AppWindow_KeyDown" back to Window
    text = re.sub(r'(<Window[^>]+)(\n\s*Title="QuickViewFile")', r'\1\n        KeyDown="AppWindow_KeyDown"\2', text)

    # Remove KeyDown="GridFileContent_KeyDown" Focusable="True" from GridFileContent
    text = re.sub(r'KeyDown="GridFileContent_KeyDown" Focusable="True">', r'>', text)

    with open(file_path, 'w') as f:
        f.write(text)

revert_keydown('QuickViewFile/MainWindow.xaml')
revert_keydown('QuickViewFile/MainWindowNoBorder.xaml')
