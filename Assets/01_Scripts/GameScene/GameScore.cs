// GameScore.cs

using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// ゲームシーンのスコア管理用クラス
/// </summary>
public class GameScore : MonoBehaviour
{
    // インスタンス(シングルトン)
    public static GameScore Instance;

    [Header("参照するUI")]
    [SerializeField] private TextMeshProUGUI txtScore;

    [Header("スコア設定")]
    [SerializeField] private int currentScore = 0;

    [Header("カウントアップ設定")]
    [SerializeField] private float countUpDuration = 0.5f;

    // 内部変数
    private int displayScore = 0;
    private Tween scoreTween;

    void Awake()
    {
        // GameScoreのインスタンスはシングルトン
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初期処理として現在スコア(0)をスコア表示に反映させる
        displayScore = currentScore;
        UpdateScoreUI(displayScore); 
    }

    void Update()
    {
        // ---- デバッグ用
        // 「Eキー」押下で+1000pt
        if(Input.GetKeyDown(KeyCode.E))
        {
            AddScore(1000);
        }

        // 「Rキー」押下で+10000pt
        if(Input.GetKeyDown(KeyCode.R))
        {
            AddScore(10000);
        }
    }

    /// <summary>
    /// スコア加算・表示更新する処理
    /// </summary>
    /// <param name="amount">加算する値</param>
    public void AddScore(int amount)
    {
        int previousScore = currentScore;
        currentScore += amount;

        // カウントアップ演出でUIを更新する
        AnimateScore(previousScore, currentScore);
    }

    /// <summary>
    /// 現在スコアを取得する処理
    /// </summary>
    /// <returns>現在スコア値 int</returns>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// スコア表示を更新する処理
    /// </summary>
    /// <returns>更新スコア値 int</returns>
    private void UpdateScoreUI(int score)
    {
        if (txtScore == null) return;

        // {0:N0}は3桁区切りのカンマ表示になる
        txtScore.text = string.Format("{0:N0}" + "pt", score);
    }

    /// <summary>
    /// スコアのカウントアップ演出処理
    /// </summary>
    /// <param name="fromScore">前回スコア値 int</param>
    /// <param name="toScore">更新スコア値 int</param>
    private void AnimateScore(int fromScore, int toScore)
    {
        // すでにアニメーション中であれば、一度停止して競合を防ぐ。
        if(scoreTween != null && scoreTween.IsActive())
        {
            scoreTween.Kill();
        }

        // fromScoreからtoScoreまで一定時間かけて数値を変化させる
        scoreTween = DOVirtual.Int(fromScore, toScore, countUpDuration, (value) =>
        {
            displayScore = value;
            UpdateScoreUI(displayScore);
        })
        .SetEase(Ease.OutQuad);
    }
}
