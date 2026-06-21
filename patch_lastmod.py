import re

with open('QuickViewFile/MainWindow.xaml', 'r') as f:
    text = f.read()

# Add Last modification date column if missing
if 'Last modification date' not in text:
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

with open('QuickViewFile/MainWindow.xaml', 'w') as f:
    f.write(text)


with open('QuickViewFile/MainWindowNoBorder.xaml', 'r') as f:
    text = f.read()

if 'Last modification date' not in text:
    text = text.replace('                    </GridView>\n                </ListView.View>', last_col + '                    </GridView>\n                </ListView.View>')

with open('QuickViewFile/MainWindowNoBorder.xaml', 'w') as f:
    f.write(text)
