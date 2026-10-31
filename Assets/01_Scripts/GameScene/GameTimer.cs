// GameTimer.cs

using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// ゲームシーンのタイマー管理用クラス
/// </summary>
public class GameTimer : MonoBehaviour
{
    [Header("GameManager参照用")]
    [SerializeField] private GameManager gameManager;

    [Header("PlayerController参照用")]
    [SerializeField] private PlayerController playerController;

    [Header("参照するUI")]
    [SerializeField] private TextMeshProUGUI txtTimer;
    [SerializeField] private GameObject imgTimeUp;

    [Header("カウントダウン用画像(3・2・1・Start)")]
    [SerializeField] private GameObject imgCount3;
    [SerializeField] private GameObject imgCount2;
    [SerializeField] private GameObject imgCount1;
    [SerializeField] private GameObject imgStart;

    [Header("タイマー設定")]
    [SerializeField] private float countdownTime = 300f;
    [SerializeField] private float timeUpDisplayDelay = 2.0f;
    [SerializeField] private float countDisplayDuration = 1.0f;

    // 内部変数
    private float currentTimer;
    private bool isTimerRunning = false;

    void Start()
    {
        currentTimer = countdownTime;  // プレイ時間の設定(5分)

        // ---- ゲームスタート前処理
        // HUDを非表示にする
        if(imgCount3 != null)
        {
            imgCount3.gameObject.SetActive(false);
        }

        if(imgCount2 != null)
        {
            imgCount2.gameObject.SetActive(false);
        }

        if (imgCount1 != null)
        {
            imgCount1.gameObject.SetActive(false);
        }

        if (imgStart != null)
        {
            imgStart.gameObject.SetActive(false);
        }

        if (imgTimeUp != null)
        {
            imgTimeUp.gameObject.SetActive(false);
        }

        // タイマーは固定表示にさせておく
        if (txtTimer != null)
        {
            txtTimer.text = " 5:00.00";
        }

        // プレイヤー操作を停止にしておく
        if (playerController != null)
        {
            playerController.DisableControl();
        }

        StartCoroutine(GameStartCountdownRoutine());  // カウントダウン演出開始
    }

    void Update()
    {
        if (!isTimerRunning) return;

        // ポーズ中は自動的にカウントは止まる
        // Time.timeScale = 0の場合は、Time.deltaTimeが0になるため。
        currentTimer -= Time.deltaTime;

        // 現在タイマーが0になった場合、タイムアップ処理が実行される。
        if(currentTimer <= 0f)
        {
            currentTimer = 0f;
            isTimerRunning = false;
            StartCoroutine(OnTimeUpRoutine());
        }

        UpdateTimerUI(); // タイマー表示更新
    }

    #region タイマー関係処理 ----

    /// <summary>
    /// タイマー表示を更新する処理
    /// </summary>
    private void UpdateTimerUI()
    {
        if (txtTimer == null) return;

        int minutes = Mathf.FloorToInt(currentTimer / 60f);  // 現在タイマー値から分を取得
        int seconds = Mathf.FloorToInt(currentTimer % 60f);  // 現在タイマー値から秒を取得

        // (現在タイマー値の小数点以下の値(0.00～0.99)) * (100) → この値を整数にする
        int fractional = Mathf.FloorToInt((currentTimer % 1f) * 100f);

        // 表示例:「01:23.45」「01:11.11」
        txtTimer.text = string.Format("{0:0}:{1:00}.{2:00}", minutes, seconds, fractional);
    }

    /// <summary>
    /// 強制タイムアップ(デバッグ用)
    /// </summary>
    public void ForceTimeUp()
    {
        if (!isTimerRunning) return;

        currentTimer = 0f;
        isTimerRunning = false;
        UpdateTimerUI();  // タイマー表示を0にする
        StartCoroutine(OnTimeUpRoutine());  // タイムアップ処理を実行
    }

    /// <summary>
    /// ゲーム開始前の「3・2・1・Start」演出処理(コルーチン)
    /// </summary>
    /// <returns>待機時間 IEnumerator</returns>
    private IEnumerator GameStartCountdownRoutine()
    {
        // 「3」の表示
        yield return StartCoroutine(PlayerCountAnimation(imgCount3));

        // 「2」の表示
        yield return StartCoroutine(PlayerCountAnimation(imgCount2));

        // 「1」の表示
        yield return StartCoroutine(PlayerCountAnimation(imgCount1));

        // 「Start」の表示
        if (imgStart != null)
        {
            yield return StartCoroutine(PlayerStartAnimation(imgStart));
        }

        isTimerRunning = true;

        // プレイヤー操作を有効にする
        if (playerController != null)
        {
            playerController.EnableControl();
        }
    }

    /// <summary>
    /// 「タイムアップ表示→リザルト画面移動」の処理(コルーチン)
    /// </summary>
    /// <returns>待機時間 IEnumerator</returns>
    private IEnumerator OnTimeUpRoutine()
    {
        AudioManager.StopBGM();

        AudioManager.PlaySETimeUp();

        // プレイヤー操作を停止にする
        if (playerController != null)
        {
            playerController.DisableControl();
        }

        // タイムアップを表示させる
        if (imgTimeUp != null)
        {
            imgTimeUp.gameObject.SetActive(true);
        }

        // 一定時間待機
        yield return new WaitForSeconds(timeUpDisplayDelay);

        // GameManager側でタイトルシーン移動処理を呼ぶ
        if (gameManager != null)
        {
            gameManager.GoToResultScene();
        }
        else
        {
            Debug.LogWarning("GameManagerがGameTimerにアタッチされていません！");
        }
    }

    #endregion // タイマー関係処理 ----

    #region アニメーション関係処理 ----

    /// <summary>
    /// 3・2・1用のDOTweenアニメーション処理(コルーチン)
    /// </summary>
    /// <param name="targetObj">アニメーションを行うオブジェクト</param>
    /// <returns>待機時間 IEnumerator</returns>
    private IEnumerator PlayerCountAnimation(GameObject targetObj)
    {
        if (targetObj == null) yield break;

        RectTransform rectTrans = targetObj.GetComponent<RectTransform>();

        AudioManager.PlaySECountDown();

        // 初期状態の設定
        targetObj.SetActive(true);
        rectTrans.localScale = Vector3.one * 1.5f;

        // 徐々に小さくしていく(最後はほぼ消えるぐらいになる)
        Sequence seq = DOTween.Sequence();
        seq.Append(rectTrans.DOScale(0.05f, countDisplayDuration).SetEase(Ease.InBack));

        // アニメーションが終わるまで待機
        yield return seq.WaitForCompletion();

        // オブジェクトを非表示にする
        targetObj.SetActive(false);
    }

    /// <summary>
    /// Start用のDOTweenアニメーション処理(コルーチン)
    /// </summary>
    /// <param name="targetObj">アニメーションを行うオブジェクト</param>
    /// <returns>待機時間 IEnumerator</returns>
    private IEnumerator PlayerStartAnimation(GameObject targetObj)
    {
        if(targetObj == null) yield break;

        RectTransform rectTrans = targetObj.GetComponent<RectTransform>();

        AudioManager.PlaySEGameStart();

        // 初期状態の設定
        targetObj.SetActive(true);
        rectTrans.localScale = Vector3.one * 0.5f;

        // 勢いよくかつ大きく表示させる
        Sequence seq = DOTween.Sequence();
        seq.Append(rectTrans.DOScale(1.5f, 0.3f).SetEase(Ease.OutBack));

        // アニメーションが終わるまで待機
        yield return seq.WaitForCompletion();

        // 大きい表示の一定時間キープする
        float displayHoldTime = Mathf.Max(0.1f, countDisplayDuration - 0.3f);
        yield return new WaitForSeconds(displayHoldTime);

        // オブジェクトを非表示にする
        targetObj.SetActive(false);
    }

    #endregion // アニメーション関係処理 ----
}
