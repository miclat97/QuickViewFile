with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'r') as f:
    text = f.read()

text = text.replace('SelectedItem?.FileContentModel = new FileContentModel', '''if (SelectedItem != null)
                    {
                        SelectedItem.FileContentModel = new FileContentModel''')
text = text.replace('                        ImageSource = null\n                    };', '                        ImageSource = null\n                    };\n                    }')

with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'w') as f:
    f.write(text)
