import re

def fix_mousedown(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        text = f.read()

    helper_code = '''
        public static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject depObj)
            {
                if (FindVisualParent<GridViewColumnHeader>(depObj) != null ||
                    FindVisualParent<ListViewItem>(depObj) != null ||
                    FindVisualParent<System.Windows.Controls.Primitives.ScrollBar>(depObj) != null ||
                    FindVisualParent<Button>(depObj) != null ||
                    FindVisualParent<TextBox>(depObj) != null)
                {
                    return;
                }
            }'''

    text = re.sub(r'private void Window_MouseLeftButtonDown\(object sender, MouseButtonEventArgs e\)\s*\{\s*try', helper_code + '\n            try', text)

    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(text)

fix_mousedown('QuickViewFile/MainWindow.xaml.cs')
