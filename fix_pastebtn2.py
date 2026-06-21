import re

with open('QuickViewFile/MainWindow.xaml', 'r') as f:
    text = f.read()

# Replace ListView Container
listview_search = r'(<ListView Grid.Column="0"\s+x:Name="FilesListView"\s+Focusable="False"\s+IsTabStop="False"\s+SelectionMode="Single"\s+ItemsSource="\{Binding ActiveListItems\}"\s+SelectedItem="\{Binding SelectedItem\}"\s+MouseDoubleClick="FilesListView_MouseDoubleClick"\s+Margin="10"\s+KeyDown="FilesListView_KeyDown"\s+ClipToBounds="True"\s+Width="Auto"\s+FocusManager.IsFocusScope="False"\s+ScrollViewer.PanningMode="VerticalFirst"\s+SelectiveScrollingGrid.SelectiveScrollingOrientation="Vertical"\s+IsManipulationEnabled="True"\s+ScrollViewer.HorizontalScrollBarVisibility="Auto"\s+ScrollViewer.CanContentScroll="True" >)'

listview_replace = r'''<Grid Grid.Column="0" Margin="10">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>
                <UniformGrid Columns="3" Rows="1" Grid.Row="0" Margin="0,0,0,5" x:Name="FileOperationsPanel">
                    <Button Content="Move" Click="MoveFiles_Click" Margin="0,0,2,0"/>
                    <Button Content="Copy" Click="CopyFiles_Click" Margin="2,0,2,0"/>
                    <Button Content="Delete" Click="DeleteFiles_Click" Margin="2,0,0,0"/>
                    <Button Content="Paste" Click="PasteFiles_Click" Margin="0,0,0,0" Visibility="Collapsed" x:Name="PasteButton" Grid.ColumnSpan="3"/>
                </UniformGrid>
            <ListView Grid.Row="1"
                  x:Name="FilesListView"
                  Focusable="True"
                  IsTabStop="True"
                  SelectionMode="Extended"
                  ItemsSource="{Binding ActiveListItems}"
                  SelectedItem="{Binding SelectedItem}"
                  MouseDoubleClick="FilesListView_MouseDoubleClick"
                  Margin="0"
                  KeyDown="FilesListView_KeyDown"
                  ClipToBounds="True"
                  Width="Auto"
                  FocusManager.IsFocusScope="False"
                  ScrollViewer.PanningMode="VerticalFirst"
                  SelectiveScrollingGrid.SelectiveScrollingOrientation="Vertical"
                  IsManipulationEnabled="True"
                  ScrollViewer.HorizontalScrollBarVisibility="Auto"
                  ScrollViewer.CanContentScroll="True" >'''

text = re.sub(listview_search, listview_replace, text)

# Close Grid
text = text.replace('            </ListView>\n            <GridSplitter Grid.Column="1"', '            </ListView>\n            </Grid>\n            <GridSplitter Grid.Column="1"')


with open('QuickViewFile/MainWindow.xaml', 'w') as f:
    f.write(text)
