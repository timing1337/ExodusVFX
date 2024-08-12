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
            Console.Init();

            ApplicationConfiguration.Initialize();
            Application.Run(new Main());
        }
    }
}