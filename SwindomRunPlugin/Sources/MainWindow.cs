using System.Diagnostics;

namespace SwindomRunPlugin;

/// <summary>
/// メインウィンドウ
/// </summary>
public class MainWindow : NativeWindow
{
    /// <summary>
    /// プロセス終了イベント
    /// </summary>
    public event EventHandler? ProcessExited;

    /// <summary>
    /// 親プロセス
    /// </summary>
    private Process _parentProcess { get; }
    /// <summary>
    /// 監視用のスレッド
    /// </summary>
    private Thread _monitorThread { get; }
    /// <summary>
    /// Dispose済みかを示すフラグ
    /// </summary>
    private bool _disposed { get; set; }

    /// <summary>
    /// HWND_MESSAGE
    /// </summary>
    private static IntPtr HWND_MESSAGE { get; } = new(-3);

    /// <summary>
    /// 「プラグイン」処理
    /// </summary>
    private PluginProcessing _pluginProcessing { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindow()
    {
        // メッセージ専用ウィンドウを作成
        var cp = new CreateParams()
        {
            Caption = "SwindomRunPlugin",
            X = 0,
            Y = 0,
            Height = 0,
            Width = 0,
            Style = 0,
            ExStyle = 0,
            Parent = HWND_MESSAGE
        };
        CreateHandle(cp);

        _pluginProcessing = new();

        // 親プロセスを監視して、終了したら自プロセスも終了
        try
        {
            _parentProcess = Process.GetProcessById(CommandLineArgsData.ParentProcessId);
            _monitorThread = new Thread(() =>
            {
                try
                {
                    _parentProcess.WaitForExit();
                    ProcessExited?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                }
            })
            {
                IsBackground = true
            };

            _monitorThread.Start();
        }
        catch (Exception ex)
        {
            ProcessExited?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// デストラクタ
    /// </summary>
    ~MainWindow()
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

        _pluginProcessing.Dispose();
        _parentProcess?.Dispose();

        _disposed = true;
    }
}
