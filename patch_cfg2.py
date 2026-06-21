with open('QuickViewFile/Models/ConfigModel.cs', 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('public double Volume { get; set; } = 1;', 'public double Volume { get; set; } = 1;\n        public int TransparentBackgroundInFullScreenMode { get; set; } = 0;')

with open('QuickViewFile/Models/ConfigModel.cs', 'w', encoding='utf-8') as f:
    f.write(text)

with open('QuickViewFile/Helpers/ConfigHelper.cs', 'r', encoding='ISO-8859-1') as f:
    text = f.read()

text = text.replace('key.SetValue(nameof(ConfigModel.Volume), config.Volume);', 'key.SetValue(nameof(ConfigModel.Volume), config.Volume);\n                key.SetValue(nameof(ConfigModel.TransparentBackgroundInFullScreenMode), config.TransparentBackgroundInFullScreenMode);')

text = text.replace('config.Volume = double.Parse((string)key.GetValue(nameof(ConfigModel.Volume).ToString(), config.Volume.ToString()));', 'config.Volume = double.Parse((string)key.GetValue(nameof(ConfigModel.Volume).ToString(), config.Volume.ToString()));\n                    config.TransparentBackgroundInFullScreenMode = (int)key.GetValue(nameof(ConfigModel.TransparentBackgroundInFullScreenMode), config.TransparentBackgroundInFullScreenMode);')

# In case the above didn't match perfectly, let's just do a regex replace
import re
text = re.sub(r'config\.Volume = double\.Parse\(\(string\)key\.GetValue\(nameof\(ConfigModel\.Volume\)\.ToString\(\), config\.Volume([^\)]*)\)\);',
              r'config.Volume = double.Parse((string)key.GetValue(nameof(ConfigModel.Volume).ToString(), config.Volume.ToString()));\n                    config.TransparentBackgroundInFullScreenMode = (int)key.GetValue(nameof(ConfigModel.TransparentBackgroundInFullScreenMode), config.TransparentBackgroundInFullScreenMode);', text)

with open('QuickViewFile/Helpers/ConfigHelper.cs', 'w', encoding='ISO-8859-1') as f:
    f.write(text)

with open('QuickViewFile/SettingsWindow.xaml.cs', 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('ConfigHelper.loadedConfig.Volume = _config.Volume;', 'ConfigHelper.loadedConfig.Volume = _config.Volume;\n            ConfigHelper.loadedConfig.TransparentBackgroundInFullScreenMode = _config.TransparentBackgroundInFullScreenMode;')

with open('QuickViewFile/SettingsWindow.xaml.cs', 'w', encoding='utf-8') as f:
    f.write(text)
