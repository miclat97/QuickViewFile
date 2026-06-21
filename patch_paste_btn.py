import re

with open('QuickViewFile/MainWindow.xaml', 'r') as f:
    text = f.read()

# Fix UniformGrid problem by replacing it with a normal Grid with Columns
grid_search = r'''<UniformGrid Columns="3" Rows="1" Grid.Row="0" Margin="0,0,0,5" x:Name="FileOperationsPanel">
                    <Button Content="Move" Click="MoveFiles_Click" Margin="0,0,2,0"/>
                    <Button Content="Copy" Click="CopyFiles_Click" Margin="2,0,2,0"/>
                    <Button Content="Delete" Click="DeleteFiles_Click" Margin="2,0,0,0"/>
                    <Button Content="Paste" Click="PasteFiles_Click" Margin="0,0,0,0" Visibility="Collapsed" x:Name="PasteButton" Grid.ColumnSpan="3"/>
                </UniformGrid>'''

grid_replace = r'''<Grid Grid.Row="0" Margin="0,0,0,5" x:Name="FileOperationsPanel">
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*" x:Name="ColMove"/>
                        <ColumnDefinition Width="*" x:Name="ColCopy"/>
                        <ColumnDefinition Width="*" x:Name="ColDelete"/>
                    </Grid.ColumnDefinitions>
                    <Button Content="Move" Click="MoveFiles_Click" Margin="0,0,2,0" Grid.Column="0" x:Name="MoveButton"/>
                    <Button Content="Copy" Click="CopyFiles_Click" Margin="2,0,2,0" Grid.Column="1" x:Name="CopyButton"/>
                    <Button Content="Delete" Click="DeleteFiles_Click" Margin="2,0,0,0" Grid.Column="2" x:Name="DeleteButton"/>
                    <Button Content="Paste" Click="PasteFiles_Click" Margin="0,0,0,0" Visibility="Collapsed" x:Name="PasteButton" Grid.Column="0" Grid.ColumnSpan="3"/>
                </Grid>'''

text = text.replace(grid_search, grid_replace)

with open('QuickViewFile/MainWindow.xaml', 'w') as f:
    f.write(text)

with open('QuickViewFile/MainWindowNoBorder.xaml', 'r') as f:
    text = f.read()
text = text.replace(grid_search, grid_replace)
with open('QuickViewFile/MainWindowNoBorder.xaml', 'w') as f:
    f.write(text)
