import re

with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'r') as f:
    text = f.read()

text = text.replace('public ObservableCollection<ItemList> ActiveListItems { get; set; } = [];', '''private ObservableCollection<ItemList> _activeListItems = [];
        public ObservableCollection<ItemList> ActiveListItems
        {
            get => _activeListItems;
            set
            {
                _activeListItems = value;
                OnPropertyChanged(nameof(ActiveListItems));
            }
        }''')

with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'w') as f:
    f.write(text)
