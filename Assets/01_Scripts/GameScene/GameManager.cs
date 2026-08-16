// GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ゲームシーン全体を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("ゲームシーンで流すBGM")]
    [SerializeField] private AudioClip gameBGM;

    [Header("ポーズウィンドウ関連")]
    [SerializeField] private GameObject pauseWindow;  // ポーズウィンドウ
    [SerializeField] private GameObject pauseFirstSelectionButton;  // ポーズウィンドウ開始時に選択されるボタン

    // 前回選択オブジェクトの記憶
    private GameObject lastSelectedObject;

    // ポーズの状態(ゲームシーン開始時はOFF)
    private bool isPaused = false;

    void Start()
    {
        AudioManager.PlayBGM(gameBGM);  // ゲームBGMを再生
    }

    void Update()
    {
        // Zキーが押された場合、リザルトシーンに移動する。
        if(Input.GetKeyDown(KeyCode.Z))
        {
            SceneManager.LoadScene("03_ResultScene");
        }

        // Escキーが押された場合
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused) // ポーズ中ならポーズを解除する
            {
                ResumeGame();
            }
            else  // ポーズ中でないならポーズ状態にする
            {
                PauseGame();
                // 選択ボタンの設定
                lastSelectedObject = UIUtils.SetFocusWithoutSound(pauseFirstSelectionButton);
            }
        }

        // ポーズ中の場合
        if(isPaused)
        {
            // キーボードやゲームパッド操作によるボタン移動を監視し移動音を鳴らす
            UIUtils.HandleselectionChangeSE(ref lastSelectedObject);
        }
    }

    #region ポーズ関係の処理 ----

    /// <summary>
    /// ポーズ開始処理
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        pauseWindow.SetActive(true);
        Time.timeScale = 0f;  // 時間を停止させる
    }

    /// <summary>
    /// ポーズ解除処理
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        pauseWindow.SetActive(false);
        Time.timeScale = 1f;  // 時間を元のスピードに戻す
    }

    /// <summary>
    /// ポーズウィンドウの「つづける」ボタンの処理
    /// ボタン押下時にポーズを解除しゲームシーンを再開させる
    /// </summary>
    public void OnContinueButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        ResumeGame();
    }

    /// <summary>
    /// ポーズウィンドウの「リトライ」ボタンの処理
    /// ボタン押下時にゲームシーンをリスタートさせる
    /// </summary>
    public void OnRetryButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        Time.timeScale = 1f;  // 時間を元のスピードに戻す
        SceneManager.LoadScene("02_GameScene");
    }

    /// <summary>
    /// ポーズウィンドウの「タイトル移動」ボタンの処理
    /// ボタン押下時にタイトルシーンへ移動する
    /// </summary>
    public void OnTitleMoveButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        Time.timeScale = 1f;  // 時間を元のスピードに戻す
        SceneManager.LoadScene("01_TitleScene");
    }

    #endregion // ポーズ関係の処理 ----
}
