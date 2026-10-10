// GameScore.cs

using UnityEngine;
using TMPro;

/// <summary>
/// ゲームシーンのスコア管理用クラス
/// </summary>
public class GameScore : MonoBehaviour
{
    // 他スクリプトからアクセス可能にするインスタンス
    public static GameScore Instance;

    [Header("参照するUI")]
    [SerializeField] private TextMeshProUGUI txtScore;

    [Header("スコア設定")]
    [SerializeField] private int currentScore = 0;

    void Awake()
    {
        // インスタンス設定(シングルトン)
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
        UpdateScoreUI();
    }

    void Update()
    {
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
    /// スコアの加算と表示更新する処理
    /// </summary>
    /// <param name="amount">加算する値</param>
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    /// <summary>
    /// 現在スコア値を取得する処理
    /// </summary>
    /// <returns>現在スコア値 int</returns>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// スコア表示を更新する処理
    /// </summary>
    private void UpdateScoreUI()
    {
        if (txtScore == null) return;

        // {0:N0}は3桁区切りのカンマ表示になる
        txtScore.text = string.Format("{0:N0}" + "pt", currentScore);
    }
}
