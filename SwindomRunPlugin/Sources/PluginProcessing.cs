using Swindom.IPluginSwindom;
using System.IO;
using System.IO.Pipes;
using System.Reflection;

namespace SwindomRunPlugin;

/// <summary>
/// プラグイン処理
/// </summary>
public class PluginProcessing
{
    /// <summary>
    /// Dispose済みかを示すフラグ
    /// </summary>
    private bool _disposed { get; set; }
    /// <summary>
    /// IPlugin
    /// </summary>
    public IPlugin? IPlugin { get; set; }
    /// <summary>
    /// ウィンドウイベント
    /// </summary>
    private FreeEcho.FEWindowEvent.WindowEvent? _windowEvent { get; set; }

    /// <summary>
    /// ウィンドウ表示を指示する文字列
    /// </summary>
    private static string _showWindowDirectionsString { get; } = "ShowWindow";
    /// <summary>
    /// プロセス終了を指示する文字列
    /// </summary>
    private static string _exitProcessDirectionsString { get; } = "ExitProcess";

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PluginProcessing()
    {
        RunPlugin();

        var progress = new Progress<string>(msg =>
        {
            IPlugin?.ShowWindow();
        });
        PipeProcessing(progress);
    }

    /// <summary>
    /// デストラクタ
    /// </summary>
    ~PluginProcessing()
    {
        Dispose(false);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(
        bool disposing
        )
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            DisposeWindowEvent();
            IPlugin?.Destruction();
        }
        _disposed = true;
    }

    /// <summary>
    /// ウィンドウイベントを破棄
    /// </summary>
    private void DisposeWindowEvent()
    {
        if (_windowEvent != null)
        {
            _windowEvent.Dispose();
            _windowEvent = null;
        }
    }

    /// <summary>
    /// プロセス間通信の処理
    /// </summary>
    private async void PipeProcessing(
        IProgress<string> progress
        )
    {
        try
        {
            using var pipeClient = new NamedPipeClientStream(".", CommandLineArgsData.PipeName, PipeDirection.In);
            await pipeClient.ConnectAsync();

            using var reader = new StreamReader(pipeClient);

            while (true)
            {
                var message = await reader.ReadLineAsync();

                if (message == _showWindowDirectionsString)
                {
                    progress.Report("ShowWindow");
                }
                else if (message == _exitProcessDirectionsString)
                {
                    Environment.Exit(0);
                }
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// プラグインを実行
    /// </summary>
    public void RunPlugin()
    {
        var pluginPath = CommandLineArgsData.PluginPath;     // プラグインファイルのパス
        if (File.Exists(pluginPath) == false)
        {
            throw new InvalidOperationException($"[{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}] There is an issue with the plugin file path.");
        }
        var settingDirectory = CommandLineArgsData.SettingDirectory;     // 設定ディレクトリ
        if (Directory.Exists(settingDirectory) == false)
        {
            throw new InvalidOperationException($"[{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}] There is a problem with the configuration directory.");
        }
        var language = CommandLineArgsData.Language;       // 言語
        var ipluginTypeName = typeof(IPlugin).FullName;       // プラグインインターフェースの型名
        if (string.IsNullOrEmpty(ipluginTypeName))
        {
            throw new InvalidOperationException($"[{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}] There is an issue with the language value.");
        }
        var assembly = Assembly.LoadFrom(pluginPath);      // アセンブリ

        foreach (Type nowType in assembly.GetTypes())
        {
            // プラグインかの確認
            if (nowType.IsClass && nowType.IsPublic
                && nowType.IsAbstract == false
                && nowType.GetInterface(ipluginTypeName) != null
                && string.IsNullOrEmpty(nowType.FullName) == false)
            {
                var objectInterface = assembly.CreateInstance(nowType.FullName)
                    ?? throw new InvalidOperationException($"[{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}] CreateInstance failed.");
                IPlugin = objectInterface as IPlugin;
                if (IPlugin == null)
                {
                    throw new InvalidOperationException($"[{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}] IPlugin is null.");
                }
                IPlugin.Initialize(settingDirectory, language);
                IPlugin.ChangeGetWindowEventTypeData.ChangeEventType += ChangeGetWindowEventTypeData_ChangeEventType;
                break;
            }
        }

        SettingsWindowEvent();
    }

    /// <summary>
    /// ウィンドウイベントを設定
    /// </summary>
    private void SettingsWindowEvent()
    {
        if (IPlugin == null)
        {
            return;
        }

        var eventType = (FreeEcho.FEWindowEvent.HookWindowEventType)IPlugin.GetWindowEventType;       // イベントの種類

        if (eventType == 0)
        {
            DisposeWindowEvent();
        }
        else
        {
            if (_windowEvent == null)
            {
                _windowEvent = new();
                _windowEvent.WindowEventOccurrence += WindowEvent_WindowEventOccurrence;
            }
            else
            {
                _windowEvent.Unhook();
            }
            _windowEvent.Hook(eventType);
        }
    }

    /// <summary>
    /// 「取得するウィンドウイベントの種類」の「ChangeEventType」イベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ChangeGetWindowEventTypeData_ChangeEventType(
        object? sender,
        ChangeGetWindowEventTypeArgs e
        )
    {
        try
        {
            SettingsWindowEvent();
        }
        catch
        {
        }
    }

    /// <summary>
    /// 「ウィンドウイベント」の「WindowEventOccurrence」イベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WindowEvent_WindowEventOccurrence(
        object? sender,
        FreeEcho.FEWindowEvent.WindowEventArgs e
        )
    {
        try
        {
            if (IPlugin == null)
            {
                return;
            }

            var windowHandle = FreeEcho.FEWindowEvent.WindowEvent.GetAncestorHwnd(e.Hwnd, e.EventType);        // ウィンドウのハンドル
            var isExistWindow = FreeEcho.FEWindowEvent.WindowEvent.ConfirmWindowVisible(windowHandle, e.EventType);      // ウィンドウが表示されているかの値

            if (IPlugin.IsWindowOnlyEventProcessing ? isExistWindow : true)
            {
                IntPtr handle = IPlugin.IsWindowOnlyEventProcessing ? windowHandle : e.Hwnd;        // ウィンドウハンドル
                IPlugin.EventProcessingData.DoEventProcessing(handle, e.EventType);
            }
        }
        catch
        {
        }
    }
}
