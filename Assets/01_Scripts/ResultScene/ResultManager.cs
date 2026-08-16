// ResultManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// リザルトシーン全体を管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    [Header("リザルトシーン開始時に選択されるボタン")]
    [SerializeField] private GameObject resultFirstSelectionButton;

    // 前回選択オブジェクトの記憶
    private GameObject lastSelectedObject;

    void Start()
    {
        AudioManager.StopBGM();  // BGMを停止
        // 選択ボタンの設定
        lastSelectedObject = UIUtils.SetFocusWithoutSound(resultFirstSelectionButton);
    }

    void Update()
    {
        // キーボードやゲームパッド操作によるボタン移動を監視し移動音を鳴らす
        UIUtils.HandleselectionChangeSE(ref lastSelectedObject);
    }

    #region 各ボタンの処理 ----

    /// <summary>
    /// 「もう一回！」ボタンの処理
    /// ボタン押下時にゲームシーンへ移動する
    /// </summary>
    public void OnOneMoreTimeButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        SceneManager.LoadScene("02_GameScene");
    }

    /// <summary>
    /// 「タイトル移動」ボタンの処理
    /// ボタン押下時にタイトルシーンへ移動する
    /// </summary>
    public void OnTitleMoveButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        SceneManager.LoadScene("01_TitleScene");
    }

    #endregion // 各ボタンの処理 ----
}