using Microsoft.Win32;
using QuickViewFile.Models;

namespace QuickViewFile.Helpers
{
    public static class ConfigHelper
    {
        private const string RegistryKeyPath = @"Software\QuickViewFile"; // Œcie¿ka do klucza rejestru HKCU
        public static readonly ConfigModel loadedConfig;

        static ConfigHelper()
        {
            loadedConfig = LoadConfig();
        }

        public static void SaveConfig(ConfigModel config)
        {
            RegistryKey? key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key == null)
            {
                return;
            }

            try
            {
                key.SetValue(nameof(ConfigModel.MaxSizePreviewKB), config.MaxSizePreviewKB);
                key.SetValue(nameof(ConfigModel.ImageStretch), config.ImageStretch);
                key.SetValue(nameof(ConfigModel.PreviewHeight), config.PreviewHeight);
                key.SetValue(nameof(ConfigModel.PreviewWidth), config.PreviewWidth);
                key.SetValue(nameof(ConfigModel.VideoHeigth), config.VideoHeigth);
                key.SetValue(nameof(ConfigModel.VideoWidth), config.VideoWidth);
                key.SetValue(nameof(ConfigModel.KeyboardZoomStep), config.KeyboardZoomStep);
                key.SetValue(nameof(ConfigModel.TextPreviewWordWrap), config.TextPreviewWordWrap);
                key.SetValue(nameof(ConfigModel.MaxScale), config.MaxScale);
                key.SetValue(nameof(ConfigModel.MinScale), config.MinScale);
                key.SetValue(nameof(ConfigModel.MouseWheelZoomStepFactor), config.MouseWheelZoomStepFactor);
                key.SetValue(nameof(ConfigModel.BitmapScalingMode), config.BitmapScalingMode);
                key.SetValue(nameof(ConfigModel.FontSize), config.FontSize);
                key.SetValue(nameof(ConfigModel.CharsToPreview), config.CharsToPreview);
                key.SetValue(nameof(ConfigModel.ImageExtensions), config.ImageExtensions);
                key.SetValue(nameof(ConfigModel.VideoExtensions), config.VideoExtensions);
                key.SetValue(nameof(ConfigModel.MusicExtensions), config.MusicExtensions);
                key.SetValue(nameof(ConfigModel.LiveStreamExtensions), config.LiveStreamExtensions);
                key.SetValue(nameof(ConfigModel.Utf8InsteadOfASCIITextPreview), config.Utf8InsteadOfASCIITextPreview);
                key.SetValue(nameof(ConfigModel.EdgeMode), config.EdgeMode);
                key.SetValue(nameof(ConfigModel.RenderMode), config.RenderMode);
                key.SetValue(nameof(ConfigModel.ShadowEffect), config.ShadowEffect);
                key.SetValue(nameof(ConfigModel.ThemeMode), config.ThemeMode);
                key.SetValue(nameof(ConfigModel.ShadowQuality), config.ShadowQuality);
                key.SetValue(nameof(ConfigModel.ShadowDepth), config.ShadowDepth);
                key.SetValue(nameof(ConfigModel.ShadowOpacity), config.ShadowOpacity);
                key.SetValue(nameof(ConfigModel.ShadowBlur), config.ShadowBlur);
                key.SetValue(nameof(ConfigModel.Volume), config.Volume);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                key.Close();
            }
        }


        public static ConfigModel LoadConfig()
        {
            ConfigModel config = new ConfigModel();
            RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);

            if (key != null)
            {
                try
                {
                    // Wczytaj wartoœci z rejestru, jeœli istniej¹, w przeciwnym razie u¿yj wartoœci domyœlnych
                    config.MaxSizePreviewKB = (int)key.GetValue(nameof(ConfigModel.MaxSizePreviewKB), config.MaxSizePreviewKB);
                    config.ImageStretch = (string)key.GetValue(nameof(ConfigModel.ImageStretch).ToString(), config.ImageStretch);
                    config.PreviewHeight = double.Parse((string)key.GetValue(nameof(ConfigModel.PreviewHeight).ToString(), config.PreviewHeight));
                    config.PreviewWidth = double.Parse((string)key.GetValue(nameof(ConfigModel.PreviewWidth).ToString(), config.PreviewWidth));
                    config.VideoHeigth = double.Parse((string)key.GetValue(nameof(ConfigModel.VideoHeigth).ToString(), config.VideoHeigth));
                    config.VideoWidth = double.Parse((string)key.GetValue(nameof(ConfigModel.VideoWidth).ToString(), config.VideoWidth));
                    config.KeyboardZoomStep = double.Parse((string)key.GetValue(nameof(ConfigModel.KeyboardZoomStep), config.KeyboardZoomStep));
                    config.TextPreviewWordWrap = (string)key.GetValue(nameof(ConfigModel.TextPreviewWordWrap).ToString(), config.TextPreviewWordWrap);
                    config.MaxScale = double.Parse((string)key.GetValue(nameof(ConfigModel.MaxScale).ToString(), config.MaxScale));
                    config.MinScale = double.Parse((string)key.GetValue(nameof(ConfigModel.MinScale).ToString(), config.MinScale));
                    config.MouseWheelZoomStepFactor = double.Parse((string)key.GetValue(nameof(ConfigModel.MouseWheelZoomStepFactor), config.MouseWheelZoomStepFactor));
                    config.BitmapScalingMode = (string)key.GetValue(nameof(ConfigModel.BitmapScalingMode).ToString(), config.BitmapScalingMode);
                    config.FontSize = double.Parse((string)key.GetValue(nameof(ConfigModel.FontSize).ToString(), config.FontSize));
                    config.CharsToPreview = double.Parse((string)key.GetValue(nameof(ConfigModel.CharsToPreview), config.CharsToPreview));
                    config.ImageExtensions = (string)key.GetValue(nameof(ConfigModel.ImageExtensions).ToString(), config.ImageExtensions);
                    config.VideoExtensions = (string)key.GetValue(nameof(ConfigModel.VideoExtensions).ToString(), config.VideoExtensions);
                    config.MusicExtensions = (string)key.GetValue(nameof(ConfigModel.MusicExtensions).ToString(), config.MusicExtensions);
                    config.LiveStreamExtensions = (string)key.GetValue(nameof(ConfigModel.LiveStreamExtensions).ToString(), config.LiveStreamExtensions);
                    config.EdgeMode = (int)key.GetValue(nameof(ConfigModel.EdgeMode), config.EdgeMode);
                    config.RenderMode = (int)key.GetValue(nameof(ConfigModel.RenderMode), config.RenderMode);
                    config.ShadowEffect = (int)key.GetValue(nameof(ConfigModel.ShadowEffect), config.ShadowEffect);
                    config.ShadowQuality = (int)key.GetValue(nameof(ConfigModel.ShadowQuality), config.ShadowQuality);
                    config.ThemeMode = (int)key.GetValue(nameof(ConfigModel.ThemeMode), config.ThemeMode);
                    config.Utf8InsteadOfASCIITextPreview = (int)key.GetValue(nameof(ConfigModel.Utf8InsteadOfASCIITextPreview), config.Utf8InsteadOfASCIITextPreview);
                    config.ShadowDepth = double.Parse((string)key.GetValue(nameof(ConfigModel.ShadowDepth).ToString(), config.ShadowDepth));
                    config.ShadowOpacity = double.Parse((string)key.GetValue(nameof(ConfigModel.ShadowOpacity).ToString(), config.ShadowOpacity));
                    config.ShadowBlur = double.Parse((string)key.GetValue(nameof(ConfigModel.ShadowBlur).ToString(), config.ShadowBlur));
                    config.Volume = double.Parse((string)key.GetValue(nameof(ConfigModel.Volume).ToString(), config.Volume));
                }
                catch (Exception ex)
                {
                    SaveConfig(config);
                }
                finally
                {
                    key.Close();
                }
            }
            else
            {
                SaveConfig(config);
            }
            return config;
        }

        public static void SetVolume(double volume)
        {
            loadedConfig.Volume = volume;
            SaveConfig(loadedConfig);
        }

        public static double GetVolume()
        {
            return loadedConfig.Volume;
        }
        public static void SetFontSize(double fontSize)
        {
            loadedConfig.FontSize = fontSize;
            SaveConfig(loadedConfig);
        }

        public static double GetFontSize()
        {
            return loadedConfig.FontSize;
        }

        public static ConfigParsedModel LoadParsedConfig()
        {
            ConfigParsedModel parsedConfig = new ConfigParsedModel
            {
                ImageExtensionsParsed = GetStringsFromCommaSeparatedString(loadedConfig.ImageExtensions),
                VideoExtensionsParsed = GetStringsFromCommaSeparatedString(loadedConfig.VideoExtensions),
                MusicExtensionsParsed = GetStringsFromCommaSeparatedString(loadedConfig.MusicExtensions),
                LiveStreamExtensionsParsed = GetStringsFromCommaSeparatedString(loadedConfig.LiveStreamExtensions)
            };
            return parsedConfig;
        }

        public static List<string> GetStringsFromCommaSeparatedString(string input)
        {
            return input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToList();
        }

        private static object? GetDefault(Type type) =>
            type.IsValueType ? Activator.CreateInstance(type) : null;

    }
}
