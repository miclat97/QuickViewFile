using QuickViewFile.Models;
using System.IO;
using System.Text.Json;

namespace QuickViewFile.Helpers
{
    public static class ConfigHelper
    {
        private const string ConfigFileName = "QuickViewFileConfig.json";

        private static readonly ConfigModel configDefault = new ConfigModel();

        private static readonly List<string> ImageStretchCorrectValues =
        [
            "Uniform",
            "UniformToFill",
            "Fill",
            "None"
        ];

        private static readonly List<string> WrapCorrectValues =
        [
            "NoWrap",
            "Wrap",
            "WrapWithOverflow"
        ];

        private static readonly List<string> BitmapScalingMode =
        [
            "Unspecified",
            "LowQuality",
            "HighQuality",
            "Linear",
            "Fant",
            "NearestNeighbor"
        ];

        public static ConfigModel LoadConfig()
        {
            ConfigModel configDefault = new();
            ConfigModel configModel = new();
            bool configIsValid = true;

            try
            {
                if (File.Exists(ConfigFileName))
                {
                    string json = File.ReadAllText(ConfigFileName);
                    configModel = JsonSerializer.Deserialize<ConfigModel>(json) ?? new ConfigModel();

                    // Uzupe³nianie braków
                    foreach (System.Reflection.PropertyInfo prop in typeof(ConfigModel).GetProperties())
                    {
                        object? currentValue = prop.GetValue(configModel);
                        object? defaultValue = prop.GetValue(configDefault);

                        // Jeœli wartoœæ jest null lub równa domyœlnej wartoœci typu (np. 0 dla int)
                        if (currentValue == null || currentValue.Equals(GetDefault(prop.PropertyType)))
                        {
                            prop.SetValue(configModel, defaultValue);
                            configIsValid = false;
                        }
                    }

                    // Walidacja pól z listami dozwolonych wartoœci
                    if (!ImageStretchCorrectValues.Contains(configModel.ImageStretch))
                    {
                        configModel.ImageStretch = configDefault.ImageStretch;
                        configIsValid = false;
                    }

                    if (!WrapCorrectValues.Contains(configModel.TextPreviewWordWrap))
                    {
                        configModel.TextPreviewWordWrap = configDefault.TextPreviewWordWrap;
                        configIsValid = false;
                    }

                    if (!BitmapScalingMode.Contains(configModel.BitmapScalingMode))
                    {
                        configModel.BitmapScalingMode = configDefault.BitmapScalingMode;
                        configIsValid = false;
                    }
                }
                else
                {
                    configIsValid = false;
                    configModel = new ConfigModel();
                }
            }
            catch
            {
                configIsValid = false;
                configModel = new ConfigModel();
            }
            finally
            {
                if (!configIsValid)
                {
                    configModel = new ConfigModel();
                }

                try
                {
                    string json = JsonSerializer.Serialize(configModel, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ConfigFileName, json);
                }
                catch
                {
                    configModel = new ConfigModel();
                }
            }

            return configModel;
        }


        private static object? GetDefault(Type type) =>
            type.IsValueType ? Activator.CreateInstance(type) : null;

    }
}
