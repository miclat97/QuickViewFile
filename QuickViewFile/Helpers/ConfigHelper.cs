using System.IO;
using System.Text.Json;
using QuickViewFile.Models;

namespace QuickViewFile
{
    public static class ConfigHelper
    {
        private const string ConfigFileName = "QuickViewFileConfig.json";

        private static List<string> ImageStretchCorrectValues = new()
        {
            "Uniform",
            "UniformToFill",
            "Fill",
            "None"
        };

        private static List<string> WrapCorrectValues = new()
        {
            "NoWrap",
            "Wrap",
            "WrapWithOverflow"
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
                    string json = JsonSerializer.Serialize(configModel, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(ConfigFileName, json);
                }
            }
            return configModel;
        }
    }
}
