using ExodusVFX.Utils;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace ExodusVFX
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            #if DEBUG
            DebugConsole.Init();
            #endif
            Log.Logger = new LoggerConfiguration().WriteTo.Console(theme: AnsiConsoleTheme.Grayscale).CreateLogger();

            ApplicationConfiguration.Initialize();
            Application.Run(new Main());
        }
    }
}