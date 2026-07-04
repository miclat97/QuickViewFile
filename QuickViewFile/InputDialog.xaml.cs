using System.Windows;

namespace QuickViewFile
{
    public partial class InputDialog : Window
    {
        public string Answer { get; private set; } = string.Empty;

        public InputDialog(string question, string defaultAnswer = "")
        {
            InitializeComponent();
            QuestionTextBlock.Text = question;
            AnswerTextBox.Text = defaultAnswer;
            AnswerTextBox.Focus();
            AnswerTextBox.SelectAll();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Answer = AnswerTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}