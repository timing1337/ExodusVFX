using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace ExodusVFX
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console(theme: AnsiConsoleTheme.Code).CreateLogger();
            #if DEBUG
            DebugConsole.Init();
            #endif

            ApplicationConfiguration.Initialize();
            Application.Run(new Main());
        }
    }
}