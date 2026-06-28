using System.Windows;

namespace QuickViewFile
{
    public partial class SlideshowSettingsWindow : Window
    {
        public string SelectedMode { get; private set; } = "Fade in/out and Random";
        public double SlideDuration { get; private set; } = 5.0;
        public double AnimDuration { get; private set; } = 0.75;
        public bool FadeOpacity { get; private set; } = true;
        public bool IsFastQuality { get; private set; } = false;

        public SlideshowSettingsWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModeComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item)
            {
                SelectedMode = item.Content.ToString() ?? "";
            }

            if (double.TryParse(SlideDurationTextBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double sd))
                SlideDuration = sd;

            if (double.TryParse(AnimDurationTextBox.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double ad))
                AnimDuration = ad;

            FadeOpacity = FadeOpacityCheckBox.IsChecked ?? true;
            IsFastQuality = QualityComboBox.SelectedIndex == 1;

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
