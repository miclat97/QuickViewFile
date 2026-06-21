with open('QuickViewFile/Models/ConfigModel.cs', 'r') as f:
    text = f.read()

text = text.replace('public double Volume { get; set; } = 1;', 'public double Volume { get; set; } = 1;\n        public int TransparentBackgroundInFullScreenMode { get; set; } = 0;')

with open('QuickViewFile/Models/ConfigModel.cs', 'w') as f:
    f.write(text)

with open('QuickViewFile/Helpers/ConfigHelper.cs', 'r') as f:
    text = f.read()

text = text.replace('key.SetValue(nameof(ConfigModel.Volume), config.Volume);', 'key.SetValue(nameof(ConfigModel.Volume), config.Volume);\n                key.SetValue(nameof(ConfigModel.TransparentBackgroundInFullScreenMode), config.TransparentBackgroundInFullScreenMode);')

text = text.replace('config.Volume = double.Parse((string)key.GetValue(nameof(ConfigModel.Volume).ToString(), config.Volume));', 'config.Volume = double.Parse((string)key.GetValue(nameof(ConfigModel.Volume).ToString(), config.Volume.ToString()));\n                    config.TransparentBackgroundInFullScreenMode = (int)key.GetValue(nameof(ConfigModel.TransparentBackgroundInFullScreenMode), config.TransparentBackgroundInFullScreenMode);')

text = text.replace('config.Volume));', 'config.Volume.ToString()));')

with open('QuickViewFile/Helpers/ConfigHelper.cs', 'w') as f:
    f.write(text)

with open('QuickViewFile/SettingsWindow.xaml.cs', 'r') as f:
    text = f.read()

text = text.replace('ConfigHelper.loadedConfig.Volume = _config.Volume;', 'ConfigHelper.loadedConfig.Volume = _config.Volume;\n            ConfigHelper.loadedConfig.TransparentBackgroundInFullScreenMode = _config.TransparentBackgroundInFullScreenMode;')

with open('QuickViewFile/SettingsWindow.xaml.cs', 'w') as f:
    f.write(text)
