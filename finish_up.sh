# 1. Update PasteLogic.cs progress updating
python3 update_logic.py

# 2. Update FilesListViewModel.cs to notify FolderPath changed
cat << 'INNER_EOF' > /tmp/vm_update.py
with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'r') as f:
    vm_content = f.read()

# Fix the RefreshFiles method to update _folderPath when called with a directory
new_refresh = '''        public void RefreshFiles(string? fileToSelect = null)
        {
            if (fileToSelect != null && System.IO.Directory.Exists(fileToSelect))
            {
                _folderPath = fileToSelect;
                fileToSelect = null; // Don't try to select a directory like a file
            }

            try
            {
                System.IO.DirectoryInfo check = new System.IO.DirectoryInfo(_folderPath);
            }'''

vm_content = vm_content.replace('''        public void RefreshFiles(string? fileToSelect = null)
        {
            try
            {
                DirectoryInfo check = new DirectoryInfo(_folderPath);
            }''', new_refresh)

# Add OnPropertyChanged("FolderPath") to the RefreshFiles so it updates the UI when navigating
vm_content = vm_content.replace('''
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                foreach (var item in ActiveListItems)
                {''', '''
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                OnPropertyChanged(nameof(FolderPath));
                foreach (var item in ActiveListItems)
                {''')

with open('QuickViewFile/ViewModel/FilesListViewModel.cs', 'w') as f:
    f.write(vm_content)
INNER_EOF
python3 /tmp/vm_update.py

# 3. Fix keyboard interception
cat << 'INNER_EOF' > /tmp/keyboard_nav.py
import re

def fix_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    new_start = '''        private void AppWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (System.Windows.Input.Keyboard.FocusedElement is System.Windows.Controls.TextBox)
            {
                return;
            }
'''

    content = re.sub(r'        private void AppWindow_KeyDown\(object sender, System\.Windows\.Input\.KeyEventArgs e\)\s*\{', new_start, content)

    with open(filepath, 'w') as f:
        f.write(content)

fix_file('QuickViewFile/MainWindow.xaml.cs')
fix_file('QuickViewFile/MainWindowNoBorder.xaml.cs')
fix_file('QuickViewFile/MainWindowThumbnails.xaml.cs')
INNER_EOF
python3 /tmp/keyboard_nav.py
