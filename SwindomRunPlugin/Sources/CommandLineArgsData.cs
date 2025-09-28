using System.IO;

namespace SwindomRunPlugin;

/// <summary>
/// コマンドライン引数のデータ
/// </summary>
public static class CommandLineArgsData
{
    /// <summary>
    /// コマンドライン引数
    /// </summary>
    private static string[] _commandLineArgs { get; } = Environment.GetCommandLineArgs();

    /// <summary>
    /// プラグインファイルのパス
    /// </summary>
    public static string PluginPath => GetArgsOrEmpty(_pluginPathIndex);
    /// <summary>
    /// 設定ディレクトリ
    /// </summary>
    public static string SettingDirectory => GetArgsOrEmpty(_settingDirectoryIndex);
    /// <summary>
    /// 言語
    /// </summary>
    public static string Language => GetArgsOrEmpty(_languageIndex);
    /// <summary>
    /// パイプ名
    /// </summary>
    public static string PipeName => GetArgsOrEmpty(_pipeNameIndex);
    /// <summary>
    /// 親プロセスID
    /// </summary>
    public static int ParentProcessId => int.Parse(GetArgsOrEmpty(_parentProcessIdIndex));

    /// <summary>
    /// プラグインファイルのパスのインデックス
    /// </summary>
    private static int _pluginPathIndex { get; } = 1;
    /// <summary>
    /// 設定ディレクトリのインデックス
    /// </summary>
    private static int _settingDirectoryIndex { get; } = 2;
    /// <summary>
    /// 言語のインデックス
    /// </summary>
    private static int _languageIndex { get; } = 3;
    /// <summary>
    /// パイプ名のインデックス
    /// </summary>
    private static int _pipeNameIndex { get; } = 4;
    /// <summary>
    /// 親プロセスIDのインデックス
    /// </summary>
    private static int _parentProcessIdIndex { get; } = 5;

    /// <summary>
    /// コマンドライン引数の数
    /// </summary>
    private static int _commandLineArgsLength { get; } = 5;

    /// <summary>
    /// インデックスを指定して文字列を取得
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <returns>文字列 (「Empty」コマンドライン引数の数が不足している)</returns>
    private static string GetArgsOrEmpty(
        int index
        )
    {
        return index < _commandLineArgs.Length ? _commandLineArgs[index] : string.Empty;
    }

    /// <summary>
    /// コマンドライン引数の確認
    /// </summary>
    /// <returns>問題ないかの値 (「false」問題がある/「true」問題ない)</returns>
    public static bool CheckArgs()
    {
        if (_commandLineArgsLength > _commandLineArgs.Length)
        {
            return false;
        }
        if (File.Exists(PluginPath) == false)
        {
            return false;
        }
        if (Directory.Exists(SettingDirectory) == false)
        {
            return false;
        }
        return true;
    }
}
