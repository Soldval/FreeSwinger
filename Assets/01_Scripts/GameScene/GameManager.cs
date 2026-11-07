// GameManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームシーン全体を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("ゲームシーンのBGM")]
    [SerializeField] private AudioClip gameBGM;

    [Header("HUD関連")]
    [SerializeField] private GameObject HUD;  // HUD(タイム, スコア, マップ)

    [Header("gameScoreの参照用")]
    [SerializeField] private GameScore gameScore;

    [Header("タイマー関連")]
    [SerializeField] private GameTimer gameTimer;

    [Header("ポーズウィンドウ関連")]
    [SerializeField] private GameObject pauseWindow;
    [SerializeField] private GameObject pauseFirstSelectionButton;

    [Header("デバッグ用設定")]
    [SerializeField] private bool isDebugMode = true;

    // 内部変数
    private GameObject lastSelectedObject;
    private bool isPaused = false;
    private bool isTimeUp = false;

    void Start()
    {
        AudioManager.PlayBGM(gameBGM);
        SetHUDActive(true);
        ResumeGame();
    }

    void Update()
    {
        // タイムアップ時は他の入力を受け付けない
        if (isTimeUp) return;

        // ---- デバッグモード時の処理
        // Zキー押下でリザルトシーンに移動する
        //if (isDebugMode && Input.GetKeyDown(KeyCode.Z))
        //{
        //    if(gameTimer != null)
        //    {
        //        gameTimer.ForceTimeUp();
        //    }
        //    else
        //    {
        //        GoToResultScene();
        //    }
        //}

        // Escキーが押された場合
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
                lastSelectedObject = UIUtils.SetFocusWithoutSound(pauseFirstSelectionButton);
            }
        }

        if(isPaused)
        {
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
    /// <param name="isActive">「表示 or 非表示」の状態</param>
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
        ResumeGame();
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
