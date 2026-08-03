using System.IO;
using System.Text;

namespace QuickViewFile.Helpers
{
    /// <summary>
    /// Opt-in diagnostics log, controlled by the DebugLogging setting (0 - Disabled, 1 - Enabled).
    ///
    /// Its main job is to capture the crashes that otherwise leave no trace: an exception thrown from a
    /// Dispatcher callback or a background thread tears the process down without a dialog and, in the case of
    /// an OutOfMemory or a failure inside the WPF text layer, often without a Windows Error Reporting entry
    /// either. Handlers only record what happened - they never mark an exception handled, so enabling the log
    /// does not change how the application behaves.
    ///
    /// Every entry is appended and flushed immediately, because the interesting entry is usually the last one
    /// written before the process dies.
    /// </summary>
    public static class DebugLog
    {
        private const string FileName = "qvf-debug.log";
        private static readonly object Gate = new object();

        private static bool _enabled;
        private static bool _handlersInstalled;
        private static bool _firstChanceHooked;
        private static string? _path;

        public static bool IsEnabled => _enabled;

        public static string Path => _path ??= ResolvePath();

        /// <summary>Installs the global handlers once and applies the configured enabled state.</summary>
        public static void Install(bool enabled)
        {
            if (!_handlersInstalled)
            {
                _handlersInstalled = true;

                System.AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    Write("UNHANDLED (terminating=" + e.IsTerminating + ")", e.ExceptionObject as System.Exception);
                };

                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
                {
                    Write("UNOBSERVED TASK", e.Exception);
                };

                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.DispatcherUnhandledException += (s, e) =>
                    {
                        // Logged only - deliberately not marked handled, so the failure still surfaces as before.
                        Write("DISPATCHER UNHANDLED", e.Exception);
                    };
                }
            }

            SetEnabled(enabled);
        }

        /// <summary>Turns logging on or off at runtime (used when the setting is changed).</summary>
        public static void SetEnabled(bool enabled)
        {
            if (_enabled == enabled) return;
            _enabled = enabled;

            // First-chance logging is noisy and costs on every thrown exception, so it is hooked only while
            // logging is on. It is what reveals exceptions that some catch block would otherwise swallow.
            if (enabled && !_firstChanceHooked)
            {
                _firstChanceHooked = true;
                System.AppDomain.CurrentDomain.FirstChanceException += OnFirstChance;
            }
            else if (!enabled && _firstChanceHooked)
            {
                _firstChanceHooked = false;
                System.AppDomain.CurrentDomain.FirstChanceException -= OnFirstChance;
            }

            if (enabled)
            {
                Write("--- logging enabled --- version "
                    + (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?")
                    + ", 64-bit=" + System.Environment.Is64BitProcess
                    + ", log=" + Path);
            }
        }

        private static void OnFirstChance(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Write("first-chance " + e.Exception.GetType().FullName + ": " + e.Exception.Message);
        }

        public static void Write(string message) => Write(message, null);

        public static void Write(string message, System.Exception? ex)
        {
            if (!_enabled) return;

            try
            {
                var sb = new StringBuilder();
                sb.Append(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sb.Append(" [t").Append(System.Environment.CurrentManagedThreadId).Append("] ");
                sb.Append("mem=").Append(System.Environment.WorkingSet / 1048576).Append("MB ");
                sb.AppendLine(message);
                if (ex != null) sb.AppendLine(ex.ToString());

                lock (Gate)
                {
                    File.AppendAllText(Path, sb.ToString());
                }
            }
            catch
            {
                // Diagnostics must never be the reason the application fails.
            }
        }

        private static string ResolvePath()
        {
            try
            {
                string dir = System.AppContext.BaseDirectory;
                string candidate = System.IO.Path.Combine(dir, FileName);
                File.AppendAllText(candidate, string.Empty); // probe: the install folder is often read-only
                return candidate;
            }
            catch
            {
                return System.IO.Path.Combine(System.IO.Path.GetTempPath(), FileName);
            }
        }
    }
}
