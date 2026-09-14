// GameTimer.cs

using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// ゲームシーンのタイマー管理用クラス
/// </summary>
public class GameTimer : MonoBehaviour
{
    [Header("参照するUI")]
    [SerializeField] private TextMeshProUGUI txtTimer;
    [SerializeField] private TextMeshProUGUI txtTimeUp;

    [Header("タイマー設定")]
    [SerializeField] private float countdownTime = 300f;
    [SerializeField] private float timeUpDisplayDelay = 2.0f;

    [Header("GameManagerを参照する")]
    [SerializeField] private GameManager gameManager;

    private float currentTimer;
    private bool isTimerRunning = false;  // タイマー実行状態

    void Start()
    {
        currentTimer = countdownTime;  // 現在タイマーに設定時間を入れる

        // 「タイムアップ」は非表示にしておく
        if(txtTimeUp != null)
        {
            txtTimeUp.gameObject.SetActive(false);
        }

        isTimerRunning = true;
    }

    void Update()
    {
        if (!isTimerRunning) return;

        // ポーズ中は自動的にカウントは止まる
        // Time.timeScale = 0の場合は、Time.deltaTimeが0になるため。
        currentTimer -= Time.deltaTime;

        // 現在タイマーが0になった場合の処理
        if(currentTimer <= 0f)
        {
            currentTimer = 0f;
            isTimerRunning = false;
            StartCoroutine(OnTimeUpRoutine());  // コルーチンを開始
        }

        UpdateTimerUI();
    }

    /// <summary>
    /// タイマー表示を更新する処理
    /// </summary>
    private void UpdateTimerUI()
    {
        // ガード節
        if (txtTimer == null) return;

        int minutes = Mathf.FloorToInt(currentTimer / 60f);  // 現在タイマー値から分を取得
        int seconds = Mathf.FloorToInt(currentTimer % 60f);  // 現在タイマー値から秒を取得

        // (現在タイマー値の小数点以下の値(0.00～0.99)) * (100) → この値を整数にする
        int fractional = Mathf.FloorToInt((currentTimer % 1f) * 100f);

        // 表示例:「01:23.45」「01:11.11」
        txtTimer.text = string.Format("{0:0}:{1:00}.{2:00}", minutes, seconds, fractional);
    }

    /// <summary>
    /// 「タイムアップ表示→リザルト画面移動」の処理(コルーチン)
    /// </summary>
    /// <returns>一時停止を行う IEnumerator</returns>
    private IEnumerator OnTimeUpRoutine()
    {
        // タイムアップを表示させる
        if(txtTimeUp != null)
        {
            txtTimeUp.text = "タイムアップ！！";
            txtTimeUp.gameObject.SetActive(true);
        }

        // 指定時間だけ一時停止する
        yield return new WaitForSeconds(timeUpDisplayDelay);

        // GameManager側でタイトルシーン移動処理を呼ぶ
        if(gameManager != null)
        {
            gameManager.GoToResultScene();
        }
    }
}
