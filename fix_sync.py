with open('QuickViewFile/MainWindow.xaml.cs', 'r') as f:
    text = f.read()

text = text.replace('FilesListView.ScrollIntoView(FilesListView.SelectedItem);\n                    e.Handled = true;', 'FilesListView.ScrollIntoView(FilesListView.SelectedItem);\n                            if (DataContext is FilesListViewModel viewm && FilesListView.SelectedItem is ItemList sel) viewm.SelectedItem = sel;\n                            e.Handled = true;')

with open('QuickViewFile/MainWindow.xaml.cs', 'w') as f:
    f.write(text)

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'r') as f:
    text = f.read()

text = text.replace('FilesListView.ScrollIntoView(FilesListView.SelectedItem);\n                    e.Handled = true;', 'FilesListView.ScrollIntoView(FilesListView.SelectedItem);\n                            if (DataContext is FilesListViewModel viewm && FilesListView.SelectedItem is ItemList sel) viewm.SelectedItem = sel;\n                            e.Handled = true;')

with open('QuickViewFile/MainWindowNoBorder.xaml.cs', 'w') as f:
    f.write(text)
