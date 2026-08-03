using System.Windows;
using QuickViewFile.Helpers;

namespace QuickViewFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DebugLog.Install(ConfigHelper.loadedConfig.DebugLogging == 1);
        }
    }

}
