// RankingDataStore.cs

using UnityEngine;
using System;

/// <summary>
/// ランキングデータを管理する共通クラス
/// </summary>
public class RankingDataStore : MonoBehaviour
{
    // インスタンス(シングルトン)
    public static RankingDataStore Instance;

    [Header("ランキングデータの保存数")]
    public int rankingDataLength = 5;

    // ランキングデータの保存配列
    [HideInInspector] public int[] scores;
    [HideInInspector] public GradeType[] grades;
    [HideInInspector] public string[] dates;

    void Awake()
    {
        InitializeArrays();

        // RankingDataStoreのインスタンスはシングルトン
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRankingData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ランキングデータの保存配列を初期化する処理
    /// </summary>
    private void InitializeArrays()
    {
        scores = new int[rankingDataLength];
        grades = new GradeType[rankingDataLength];
        dates = new string[rankingDataLength];

        for(int i = 0; i < rankingDataLength; i++)
        {
            scores[i] = -1;
            grades[i] = GradeType.None;
            dates[i] = "";
        }
    }

    /// <summary>
    /// スコアチェックしてランキング登録しPlayerPrefsに保存する処理
    /// </summary>
    /// <param name="newScore">スコア結果</param>
    /// <param name="newGrade">成績結果</param>
    /// <returns>ランクインの順位 int</returns>
    public int CheckAndRegisterRanking(int resultScore, GradeType resultGrade)
    {
        LoadRankingData();

        int rankIndex = -1;

        // ---- 上から順に比較し今回スコアが高い場所を探す
        for (int i = 0; i < rankingDataLength; i++)
        {
            // scores[i] < 0 は未登録状態なので無条件でランクインする
            if (scores[i] < 0 || resultScore > scores[i])
            {
                rankIndex = i;
                break;
            }
        }

        // ---- ランクインする場合
        if (rankIndex >= 0)
        {
            // 既存のデータを下位へ押し出す
            for (int i = rankingDataLength - 1; i > rankIndex; i--)
            {
                scores[i] = scores[i - 1];
                grades[i] = grades[i - 1];
                dates[i] = dates[i - 1];
            }

            // 新しいデータを挿入
            scores[rankIndex] = resultScore;
            grades[rankIndex] = resultGrade;
            dates[rankIndex] = DateTime.Now.ToString("yyyy/MM/dd");

            SaveRankingData();
        }

        return rankIndex;
    }

    /// <summary>
    /// ランキングデータをPlayerPrefsに保存する処理
    /// </summary>
    public void SaveRankingData()
    {
        for (int i = 0; i < rankingDataLength; i++)
        {
            PlayerPrefs.SetInt($"RANKING_SCORE_{i}", scores[i]);
            PlayerPrefs.SetInt($"RANKING_GRADE_{i}", (int)grades[i]);
            PlayerPrefs.SetString($"RANKING_DATE_{i}", dates[i]);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// PlayerPrefsからランキングデータをロードする処理
    /// </summary>
    public void LoadRankingData()
    {
        // 配列サイズが変化している場合はランキングデータの再初期化を行う
        if(scores == null || scores.Length != rankingDataLength)
        {
            InitializeArrays();
        }

        for (int i = 0; i < rankingDataLength; i++)
        {
            // 初期値がない場合は、スコア:-1、成績:None、日付:空文字 をロードする。
            scores[i] = PlayerPrefs.GetInt($"RANKING_SCORE_{i}", -1);
            grades[i] = (GradeType)PlayerPrefs.GetInt($"RANKING_GRADE_{i}", (int)GradeType.None);
            dates[i] = PlayerPrefs.GetString($"RANKING_DATE_{i}", "");
        }
    }
}
