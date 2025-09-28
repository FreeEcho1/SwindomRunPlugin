using System.Reflection;

namespace SwindomRunPlugin;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    /// <summary>
    /// システムトレイアイコンのウィンドウ
    /// </summary>
    private MainWindow _mainWindow { get; set; }

    /// <summary>
    /// /Application.Startupイベント
    /// </summary>
    /// <param name="e"></param>
    protected override void OnStartup(
        System.Windows.StartupEventArgs e
        )
    {
        base.OnStartup(e);
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

        try
        {
            if (CommandLineArgsData.CheckArgs() == false)
            {
                throw new ArgumentException($"[{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}] There is an issue with the command-line arguments.");
            }

            _mainWindow = new();
            _mainWindow.ProcessExited += (_, __) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Shutdown();
                });
            };
        }
        catch
        {
            Shutdown();
        }
    }

    /// <summary>
    /// Application.Exitイベント
    /// </summary>
    /// <param name="e"></param>
    protected override void OnExit(
        System.Windows.ExitEventArgs e
        )
    {
        try
        {
            _mainWindow.Dispose();
            base.OnExit(e);
        }
        catch
        {
        }
    }
}
