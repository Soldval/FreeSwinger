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

    [Header("スコア関連")]
    [SerializeField] private GameScore gameScore;  // スコア管理クラスを参照

    [Header("タイマー関連")]
    [SerializeField] private GameTimer gameTimer;

    [Header("ポーズウィンドウ関連")]
    [SerializeField] private GameObject pauseWindow;  // ポーズウィンドウ
    [SerializeField] private GameObject pauseFirstSelectionButton;  // ポーズウィンドウ開始時に選択されるボタン

    [Header("デバッグ用設定")]
    [SerializeField] private bool isDebugMode = true;  // デバッグモード

    // 内部変数リスト
    private GameObject lastSelectedObject;

    private bool isPaused = false;
    private bool isTimeUp = false;

    void Start()
    {
        AudioManager.PlayBGM(gameBGM);
        SetHUDActive(true);
        ResumeGame();  // ポーズを解除する
    }

    void Update()
    {
        // タイムアップ時は他の入力を受け付けない
        if (isTimeUp) return;

        // デバッグモード時の処理
        // Zキー押下でリザルトシーンに移動する
        if (isDebugMode && Input.GetKeyDown(KeyCode.Z))
        {
            if(gameTimer != null)
            {
                gameTimer.ForceTimeUp();
            }
            else
            {
                GoToResultScene();
            }
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
        // タイムアップ処理の二重実行を防ぐ
        if (isTimeUp) return;
        isTimeUp = true;

        Time.timeScale = 1f;
        SetHUDActive(false);

        // 現在スコアをPlayerPrefsに保存
        if(gameScore != null)
        {
            int currentScore = gameScore.GetCurrentScore();
            PlayerPrefs.SetInt("FINAL_SCORE", currentScore);
            PlayerPrefs.Save();  // 確実に書き込む
        }
        else
        {
            Debug.LogWarning("gameScoreがアタッチされてないため、スコア保存できませんでした。");
        }

        SceneManager.LoadScene("03_ResultScene");
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
        isPaused = true;
        pauseWindow.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// ポーズ解除処理
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        pauseWindow.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// ポーズウィンドウの「つづける」ボタンの処理
    /// ボタン押下時にポーズを解除しゲームシーンを再開させる
    /// </summary>
    public void OnContinueButtonPress()
    {
        AudioManager.PlaySESubmit();
        ResumeGame();  // ポーズを解除する
    }

    /// <summary>
    /// ポーズウィンドウの「リトライ」ボタンの処理
    /// ボタン押下時にゲームシーンをリスタートさせる
    /// </summary>
    public void OnRetryButtonPress()
    {
        AudioManager.PlaySESubmit();
        Time.timeScale = 1f;
        SetHUDActive(false);
        SceneBridge.LoadSceneWithLoading("02_GameScene");
    }

    /// <summary>
    /// ポーズウィンドウの「タイトル移動」ボタンの処理
    /// ボタン押下時にタイトルシーンへ移動する
    /// </summary>
    public void OnTitleMoveButtonPress()
    {
        AudioManager.PlaySESubmit();
        Time.timeScale = 1f;
        SetHUDActive(false);
        SceneBridge.LoadSceneWithLoading("01_TitleScene");
    }

    #endregion // ポーズ関係の処理 ----
}
