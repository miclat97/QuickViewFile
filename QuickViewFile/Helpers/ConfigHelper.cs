using QuickViewFile.Models;
using System.IO;
using System.Text.Json;

namespace QuickViewFile
{
    public static class ConfigHelper
    {
        private const string ConfigFileName = "QuickViewFileConfig.json";

        private static readonly List<string> ImageStretchCorrectValues = new()
        {
            "Uniform",
            "UniformToFill",
            "Fill",
            "None"
        };

        private static readonly List<string> WrapCorrectValues = new()
        {
            "NoWrap",
            "Wrap",
            "WrapWithOverflow"
        };

        private static readonly List<string> PreviewScrollBarsCorrectValues = new()
        {
            "Disabled",
            "Auto",
            "Hidden",
            "Visible"
        };

        public static ConfigModel LoadConfig()
        {
            bool configIsValid = true;
            ConfigModel configModel = new ConfigModel();

            try
            {
                if (File.Exists(ConfigFileName))
                {
                    string json = File.ReadAllText(ConfigFileName);
                    configModel = JsonSerializer.Deserialize<ConfigModel>(json);

                    if (!ImageStretchCorrectValues.Contains(configModel.ImageStretch))
                        configIsValid = false;

                    if (!WrapCorrectValues.Contains(configModel.TextPreviewWordWrap))
                        configIsValid = false;

                    if (!PreviewScrollBarsCorrectValues.Contains(configModel.PreviewScrollBars))
                        configIsValid = false;
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
                    try
                    {
                        string json = JsonSerializer.Serialize(configModel, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(ConfigFileName, json);
                    }
                    catch
                    {
                        // If writing the config file fails, We just doesn't save it and method will return ConfigModel with default values
                        configModel = new ConfigModel(); // but make sure to reset loaded every possibly wrong config field
                    }
                }
            }
            return configModel;
        }
    }
}
