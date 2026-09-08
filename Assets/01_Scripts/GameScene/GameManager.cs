// GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームシーン全体を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("ゲームシーンで流すBGM")]
    [SerializeField] private AudioClip gameBGM;

    [Header("HUD関連")]
    [SerializeField] private GameObject HUD;  // HUD(タイム, スコア, マップ)

    [Header("ポーズウィンドウ関連")]
    [SerializeField] private GameObject pauseWindow;  // ポーズウィンドウ
    [SerializeField] private GameObject pauseFirstSelectionButton;  // ポーズウィンドウ開始時に選択されるボタン

    [Header("デバッグ用設定")]
    [SerializeField] private bool isDebugMode = false;  // デバッグモード

    // 前回選択オブジェクトの記憶
    private GameObject lastSelectedObject;

    // ポーズ状態(ゲームシーン開始時はOFF)
    private bool isPaused = false;

    void Start()
    {
        AudioManager.PlayBGM(gameBGM);  // ゲームBGMを再生
        SetHUDActive(true);  // HUDを表示させる
        ResumeGame();  // ポーズウィンドウを非表示にする
    }

    void Update()
    {
        // デバッグモード時の処理
        // Zキー押下でリザルトシーンに移動する
        if(isDebugMode && Input.GetKeyDown(KeyCode.Z))
        {
            GoToResultScene();
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

    /// <summary>
    /// リザルトシーンへ移動する処理
    /// </summary>
    public void GoToResultScene()
    {
        Time.timeScale = 1f;  // 一応時間を戻しておく
        SetHUDActive(false);  // HUDを非表示にする
        SceneManager.LoadScene("03_ResultScene");  // リザルトシーンへ移動
    }

    /// <summary>
    /// HUDの表示/非表示を操作する処理
    /// </summary>
    /// <param name="isActive"></param>
    public void SetHUDActive(bool isActive)
    {
        if(HUD != null)
        {
            HUD.SetActive(isActive);
        }
    }

    #region ポーズ関係の処理 ----

    /// <summary>
    /// ポーズ開始処理
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;  // ポーズ状態をON
        pauseWindow.SetActive(true);  // ポーズウィンドウを表示させる
        Time.timeScale = 0f;  // 時間を停止させる
    }

    /// <summary>
    /// ポーズ解除処理
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;  // ポーズ状態をOFF
        pauseWindow.SetActive(false);  // ポーズウィンドウを非表示にする
        Time.timeScale = 1f;  // 時間を元のスピードに戻す
    }

    /// <summary>
    /// ポーズウィンドウの「つづける」ボタンの処理
    /// ボタン押下時にポーズを解除しゲームシーンを再開させる
    /// </summary>
    public void OnContinueButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        ResumeGame();  // ゲームシーンを再開させる
    }

    /// <summary>
    /// ポーズウィンドウの「リトライ」ボタンの処理
    /// ボタン押下時にゲームシーンをリスタートさせる
    /// </summary>
    public void OnRetryButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        Time.timeScale = 1f;  // 時間を元のスピードに戻す
        SetHUDActive(false);  // HUDを非表示にする
        SceneManager.LoadScene("02_GameScene");  // ゲームシーンをリスタート
    }

    /// <summary>
    /// ポーズウィンドウの「タイトル移動」ボタンの処理
    /// ボタン押下時にタイトルシーンへ移動する
    /// </summary>
    public void OnTitleMoveButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        Time.timeScale = 1f;  // 時間を元のスピードに戻す
        SetHUDActive(false);  // HUDを非表示にする
        SceneManager.LoadScene("01_TitleScene");  // タイトルシーンへ移動
    }

    #endregion // ポーズ関係の処理 ----
}
