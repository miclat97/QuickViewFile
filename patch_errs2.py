import re

with open('QuickViewFile/MainWindow.xaml.cs', 'r') as f:
    text = f.read()

# Fix PasteButton not found by adding an empty check if it is not visible due to it missing in the main window list grid when copied?
# Oh wait, we replaced the grid content with the PasteButton, but in C# code we might have missed naming it `PasteButton` in MainWindowNoBorder.xaml... wait, checking the error log:
# MainWindow.xaml.cs: error CS0103: The name 'PasteButton' does not exist in the current context
# This implies the name 'PasteButton' is missing from `MainWindow.xaml` or isn't picked up.
