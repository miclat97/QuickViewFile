import re

with open('QuickViewFile/MainWindowNoBorder.xaml', 'r') as f:
    text = f.read()

# Add standard buttons (Settings, F4)
topinfo_search = r'(<Grid Grid.Row="0" x:Name="TopInfoPanel">\s*<Grid.ColumnDefinitions>\s*<ColumnDefinition Width="\*"/>\s*<ColumnDefinition Width="Auto"/>\s*</Grid.ColumnDefinitions>\s*<TextBlock[\s\S]*?TextWrapping="NoWrap" />)'
topinfo_replace = r'''\1
            <StackPanel Grid.Column="1" Orientation="Horizontal" HorizontalAlignment="Right">
                <Button Content="Exit Full Screen" Name="ExitFullScreenButton" Height="24" Margin="0,8,10,8" Padding="10,0" Click="ExitFullScreenButton_Click" ToolTip="TIP: F4 can do this"/>
                <Button Content="Settings" Name="SettingsButton" Height="24" Margin="0,8,10,8" Padding="10,0" Click="SettingsButton_Click" ToolTip="Open Settings"/>'''
text = re.sub(topinfo_search, topinfo_replace, text)


save_btn = r'''<Button
              Grid.Column="1"
              Content="Save"
              Name="SaveButton"
              Height="24"
              Margin="0,8,10,8"
              Padding="15,0"
              Click="SaveButton_Click"
              Visibility="\{Binding SelectedItem.FileContentModel.ShowTextBox, Converter=\{StaticResource BoolToVisibilityConverter\}\}"/>'''

text = re.sub(save_btn, '''<Button Content="Save" Name="SaveButton" Height="24" Margin="0,8,10,8" Padding="15,0" Click="SaveButton_Click" Visibility="{Binding SelectedItem.FileContentModel.ShowTextBox, Converter={StaticResource BoolToVisibilityConverter}}"/>\n            </StackPanel>''', text)

# Replace ListView Container
listview_search = r'(<ListView Grid.Column="0"\s+x:Name="FilesListView"\s+Focusable="False"\s+IsTabStop="False"\s+SelectionMode=")Single("\s+ItemsSource="\{Binding ActiveListItems\}"\s+SelectedItem="\{Binding SelectedItem\}"\s+MouseDoubleClick="FilesListView_MouseDoubleClick"\s+Margin=")10("\s+KeyDown="FilesListView_KeyDown"\s+ClipToBounds="True"\s+Width=")Auto("\s+FocusManager.IsFocusScope="False"\s+ScrollViewer.PanningMode="VerticalFirst"\s+SelectiveScrollingGrid.SelectiveScrollingOrientation="Vertical"\s+IsManipulationEnabled="True"\s+ScrollViewer.HorizontalScrollBarVisibility="Auto"\s+ScrollViewer.CanContentScroll="True" >)'

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

# Replace GridViewColumn Name Header
text = text.replace(
    '<GridViewColumn Header="Name" Width="Auto" FocusManager.IsFocusScope="False">',
    '<GridViewColumn Width="Auto" FocusManager.IsFocusScope="False">\n                            <GridViewColumn.Header>\n                                <GridViewColumnHeader Content="Name" Tag="Name" Click="ColumnHeader_Click"/>\n                            </GridViewColumn.Header>'
)

# Replace GridViewColumn Size Header
text = text.replace(
    '<GridViewColumn Header="Size (KB)" Width="80" FocusManager.IsFocusScope="False">',
    '<GridViewColumn Width="80" FocusManager.IsFocusScope="False">\n                            <GridViewColumn.Header>\n                                <GridViewColumnHeader Content="Size (KB)" Tag="Size" Click="ColumnHeader_Click"/>\n                            </GridViewColumn.Header>'
)

# Add Last modification date column
last_col = '''
                        <GridViewColumn Width="150" FocusManager.IsFocusScope="False">
                            <GridViewColumn.Header>
                                <GridViewColumnHeader Content="Last modification date" Tag="LastModified" Click="ColumnHeader_Click"/>
                            </GridViewColumn.Header>
                            <GridViewColumn.CellTemplate>
                                <DataTemplate>
                                    <TextBlock Text="{Binding LastModifiedString}"
                                               FontSize="13"
                                               Focusable="False"
                                               VerticalAlignment="Center"
                                               FontWeight="Normal"/>
                                </DataTemplate>
                            </GridViewColumn.CellTemplate>
                        </GridViewColumn>
'''

text = text.replace('                    </GridView>\n                </ListView.View>', last_col + '                    </GridView>\n                </ListView.View>')


# Add Grid Closing
text = text.replace('            </ListView>\n            <GridSplitter Grid.Column="1"', '            </ListView>\n            </Grid>\n            <GridSplitter Grid.Column="1"')

# Also remove KeyDown from Window tag
text = re.sub(r'KeyDown="AppWindow_KeyDown"\n\s*', '', text)

text = text.replace(
'''<Grid MouseRightButtonDown="FileContentGrid_MouseRightButtonDown"
                          Grid.Row="0"
                          Name="GridFileContent" ClipToBounds="True" Grid.IsSharedSizeScope="True" IsManipulationEnabled="True" SnapsToDevicePixels="True">''',
'''<Grid MouseRightButtonDown="FileContentGrid_MouseRightButtonDown"
                          Grid.Row="0"
                          Name="GridFileContent" ClipToBounds="True" Grid.IsSharedSizeScope="True" IsManipulationEnabled="True" SnapsToDevicePixels="True"
                          KeyDown="GridFileContent_KeyDown" Focusable="True">''')

with open('QuickViewFile/MainWindowNoBorder.xaml', 'w') as f:
    f.write(text)
