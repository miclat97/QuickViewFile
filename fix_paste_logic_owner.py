with open('QuickViewFile/PasteLogic.cs', 'r') as f:
    content = f.read()

# Make the OverwriteDialog.Owner assignment conditional on whether ownerWindow is visible
# Sometimes MainWindowNoBorder is running in a context where setting it as Owner throws an exception if it's not the active top window or if IsVisible is false.
new_dialog_code = '''                                    var overwriteDialog = new OverwriteDialog($"'{itemName}' already exists. What do you want to do?");
                                    if (ownerWindow != null && ownerWindow.IsVisible)
                                    {
                                        overwriteDialog.Owner = ownerWindow;
                                    }
                                    overwriteDialog.ShowDialog();'''

content = content.replace('''                                    var overwriteDialog = new OverwriteDialog($"'{itemName}' already exists. What do you want to do?");
                                    overwriteDialog.Owner = ownerWindow;
                                    overwriteDialog.ShowDialog();''', new_dialog_code)

with open('QuickViewFile/PasteLogic.cs', 'w') as f:
    f.write(content)
