using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using QuickViewFile.Helpers;
using QuickViewFile.Models;

namespace QuickViewFile
{
    public partial class SettingsWindow : Window
    {
        private ConfigModel _config;

        public SettingsWindow()
        {
            InitializeComponent();
            _config = ConfigHelper.LoadConfig();
            GenerateUI();
        }

        private void GenerateUI()
        {
            PropertyInfo[] properties = typeof(ConfigModel).GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.Name == "BitmapScalingMode_") continue; // Skip internal or unwanted properties

                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };

                var label = new TextBlock
                {
                    Text = prop.Name,
                    Width = 250,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                panel.Children.Add(label);

                if (prop.Name == "ThemeMode")
                {
                    var comboBox = new ComboBox { Width = 400, VerticalAlignment = VerticalAlignment.Center };
                    comboBox.Items.Add(new ComboBoxItem { Content = "Auto", Tag = 0 });
                    comboBox.Items.Add(new ComboBoxItem { Content = "Light", Tag = 1 });
                    comboBox.Items.Add(new ComboBoxItem { Content = "Dark", Tag = 2 });
                    comboBox.SelectedValuePath = "Tag";
                    var binding = new Binding(prop.Name) { Source = _config, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay };
                    comboBox.SetBinding(ComboBox.SelectedValueProperty, binding);
                    panel.Children.Add(comboBox);
                }
                else if (prop.Name == "BitmapScalingMode")
                {
                    var comboBox = new ComboBox { Width = 400, VerticalAlignment = VerticalAlignment.Center };
                    comboBox.Items.Add(new ComboBoxItem { Content = "Unspecified", Tag = "Unspecified" });
                    comboBox.Items.Add(new ComboBoxItem { Content = "Linear", Tag = "Linear" });
                    comboBox.Items.Add(new ComboBoxItem { Content = "LowQuality", Tag = "LowQuality" });
                    comboBox.Items.Add(new ComboBoxItem { Content = "Fant", Tag = "Fant" });
                    comboBox.Items.Add(new ComboBoxItem { Content = "HighQuality", Tag = "HighQuality" });
                    comboBox.Items.Add(new ComboBoxItem { Content = "NearestNeighbor", Tag = "NearestNeighbor" });
                    comboBox.SelectedValuePath = "Tag";
                    var binding = new Binding(prop.Name) { Source = _config, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay };
                    comboBox.SetBinding(ComboBox.SelectedValueProperty, binding);
                    panel.Children.Add(comboBox);
                }
                else if (prop.Name == "ShadowEffect" || prop.Name == "ShadowQuality" || prop.Name == "RenderMode" ||
                         prop.Name == "EdgeMode" || prop.Name == "Utf8InsteadOfASCIITextPreview" || prop.Name == "TransparentBackgroundInFullScreenMode")
                {
                    var comboBox = new ComboBox { Width = 400, VerticalAlignment = VerticalAlignment.Center };

                    if (prop.Name == "Utf8InsteadOfASCIITextPreview" || prop.Name == "TransparentBackgroundInFullScreenMode" || prop.Name == "ShadowEffect")
                    {
                        comboBox.Items.Add(new ComboBoxItem { Content = "Disabled", Tag = 0 });
                        comboBox.Items.Add(new ComboBoxItem { Content = "Enabled", Tag = 1 });
                    }
                    else if (prop.Name == "ShadowQuality")
                    {
                        comboBox.Items.Add(new ComboBoxItem { Content = "Performance", Tag = 0 });
                        comboBox.Items.Add(new ComboBoxItem { Content = "Quality", Tag = 1 });
                    }
                    else if (prop.Name == "RenderMode")
                    {
                        comboBox.Items.Add(new ComboBoxItem { Content = "Default", Tag = 0 });
                        comboBox.Items.Add(new ComboBoxItem { Content = "SoftwareOnly", Tag = 1 });
                    }
                    else if (prop.Name == "EdgeMode")
                    {
                        comboBox.Items.Add(new ComboBoxItem { Content = "Unspecified", Tag = 0 });
                        comboBox.Items.Add(new ComboBoxItem { Content = "Aliased", Tag = 1 });
                    }

                    comboBox.SelectedValuePath = "Tag";
                    var binding = new Binding(prop.Name) { Source = _config, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, Mode = BindingMode.TwoWay };
                    comboBox.SetBinding(ComboBox.SelectedValueProperty, binding);
                    panel.Children.Add(comboBox);
                }
                else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(string))
                {
                    var textBox = new TextBox { Width = 400, VerticalAlignment = VerticalAlignment.Center };
                    var binding = new Binding(prop.Name)
                    {
                        Source = _config,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay
                    };
                    textBox.SetBinding(TextBox.TextProperty, binding);
                    panel.Children.Add(textBox);
                }

                SettingsPanel.Children.Add(panel);
            }
        }


        private void RestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            _config = new ConfigModel();
            SettingsPanel.Children.Clear();
            GenerateUI();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigHelper.SaveConfig(_config);
            ConfigHelper.loadedConfig.MaxSizePreviewKB = _config.MaxSizePreviewKB;
            ConfigHelper.loadedConfig.ImageStretch = _config.ImageStretch;
            ConfigHelper.loadedConfig.PreviewHeight = _config.PreviewHeight;
            ConfigHelper.loadedConfig.PreviewWidth = _config.PreviewWidth;
            ConfigHelper.loadedConfig.VideoHeigth = _config.VideoHeigth;
            ConfigHelper.loadedConfig.VideoWidth = _config.VideoWidth;
            ConfigHelper.loadedConfig.KeyboardZoomStep = _config.KeyboardZoomStep;
            ConfigHelper.loadedConfig.TextPreviewWordWrap = _config.TextPreviewWordWrap;
            ConfigHelper.loadedConfig.MaxScale = _config.MaxScale;
            ConfigHelper.loadedConfig.MinScale = _config.MinScale;
            ConfigHelper.loadedConfig.MouseWheelZoomStepFactor = _config.MouseWheelZoomStepFactor;
            ConfigHelper.loadedConfig.BitmapScalingMode = _config.BitmapScalingMode;
            ConfigHelper.loadedConfig.FontSize = _config.FontSize;
            ConfigHelper.loadedConfig.CharsToPreview = _config.CharsToPreview;
            ConfigHelper.loadedConfig.ImageExtensions = _config.ImageExtensions;
            ConfigHelper.loadedConfig.VideoExtensions = _config.VideoExtensions;
            ConfigHelper.loadedConfig.MusicExtensions = _config.MusicExtensions;
            ConfigHelper.loadedConfig.LiveStreamExtensions = _config.LiveStreamExtensions;
            ConfigHelper.loadedConfig.Utf8InsteadOfASCIITextPreview = _config.Utf8InsteadOfASCIITextPreview;
            ConfigHelper.loadedConfig.ShadowEffect = _config.ShadowEffect;
            ConfigHelper.loadedConfig.ShadowQuality = _config.ShadowQuality;
            ConfigHelper.loadedConfig.RenderMode = _config.RenderMode;
            ConfigHelper.loadedConfig.EdgeMode = _config.EdgeMode;
            ConfigHelper.loadedConfig.ThemeMode = _config.ThemeMode;
            ConfigHelper.loadedConfig.ShadowDepth = _config.ShadowDepth;
            ConfigHelper.loadedConfig.ShadowOpacity = _config.ShadowOpacity;
            ConfigHelper.loadedConfig.ShadowBlur = _config.ShadowBlur;
            ConfigHelper.loadedConfig.Volume = _config.Volume;
            ConfigHelper.loadedConfig.TransparentBackgroundInFullScreenMode = _config.TransparentBackgroundInFullScreenMode;

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
