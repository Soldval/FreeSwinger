// SceneBridge.cs

/// <summary>
/// シーン移動用の共通クラス
/// </summary>
public static class SceneBridge
{
    // 次に読み込むシーン名
    public static string NextSceneName { get; private set; } = string.Empty;

    /// <summary>
    /// ロード画面を経由し次のシーンへ移動する処理
    /// </summary>
    /// <param name="nextScene">次の移動先シーン名</param>
    public static void LoadSceneWithLoading(string nextScene)
    {
        NextSceneName = nextScene;
        UnityEngine.SceneManagement.SceneManager.LoadScene("04_LoadingScene");
    }
}