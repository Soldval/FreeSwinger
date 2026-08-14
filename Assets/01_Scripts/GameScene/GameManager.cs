// GameManager.cs

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲームシーン全体を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("ゲームシーンで流すBGM")]
    [SerializeField] private AudioClip gameBGM;

    // ポーズウィンドウ(フォルダ)を登録
    [SerializeField] private GameObject PauseWindow;

    // ポーズの状態
    private bool isPaused = false;

    void Start()
    {
        // ゲームシーン開始時にゲームシーンBGMを再生させる
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(gameBGM);
        }
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
            if(isPaused)  // ポーズ中だったらゲームに戻る
            {
                ResumeGame();
            }
            else  // ポーズ中ではないならポーズする
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        PauseWindow.SetActive(true);

        // 時間の進みを停止にする
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        PauseWindow.SetActive(false);

        // 時間の進みを通常スピードに戻す
        Time.timeScale = 1f;
    }
}
