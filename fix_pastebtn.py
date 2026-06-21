import re

with open('QuickViewFile/MainWindow.xaml', 'r') as f:
    text = f.read()

# I notice that `MainWindow.xaml` replacement for ListView was slightly mangled or didn't go through properly because in our earlier `patch_col.py` or similar we replaced `Grid.Row="1"` but lost `UniformGrid`. Let's re-add the UniformGrid correctly.

search = r'(<ListView Grid.Row="1"\s+x:Name="FilesListView")'
replace = r'''<UniformGrid Columns="3" Rows="1" Grid.Row="0" Margin="0,0,0,5" x:Name="FileOperationsPanel">
                    <Button Content="Move" Click="MoveFiles_Click" Margin="0,0,2,0"/>
                    <Button Content="Copy" Click="CopyFiles_Click" Margin="2,0,2,0"/>
                    <Button Content="Delete" Click="DeleteFiles_Click" Margin="2,0,0,0"/>
                    <Button Content="Paste" Click="PasteFiles_Click" Margin="0,0,0,0" Visibility="Collapsed" x:Name="PasteButton" Grid.ColumnSpan="3"/>
                </UniformGrid>
                \1'''

if '<UniformGrid Columns="3"' not in text:
    text = re.sub(search, replace, text)

with open('QuickViewFile/MainWindow.xaml', 'w') as f:
    f.write(text)
