using System.Windows;

namespace QuickViewFile
{
    public enum OverwriteAction { Replace, AutoRename, Skip }

    public partial class OverwriteDialog : Window
    {
        public OverwriteAction SelectedAction { get; private set; } = OverwriteAction.Skip;
        public bool DoForAll => DoForAllCheckBox.IsChecked == true;

        public OverwriteDialog(string message)
        {
            InitializeComponent();
            QuestionTextBlock.Text = message;
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = OverwriteAction.Replace;
            DialogResult = true;
        }

        private void AutoRenameButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = OverwriteAction.AutoRename;
            DialogResult = true;
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = OverwriteAction.Skip;
            DialogResult = false;
        }
    }
}