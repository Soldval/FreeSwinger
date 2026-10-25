// RankingDataStore.cs

using UnityEngine;
using System;

/// <summary>
/// ランキングデータを管理するクラス
/// </summary>
public class RankingDataStore : MonoBehaviour
{
    public static RankingDataStore Instance;

    // ランキングデータの保存配列
    public int[] scores = new int[5] { -1, -1, -1, -1, -1 };
    public GradeType[] grades = new GradeType[5] { GradeType.None, GradeType.None, GradeType.None, GradeType.None, GradeType.None };
    public string[] dates = new string[5] { "", "", "", "", "" };

    void Awake()
    {
        // インスタンスはシングルトン
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRankingData();  // 起動時にロードする
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// スコアチェックしてランキング登録しPlayerPrefsに保存する処理
    /// </summary>
    /// <param name="newScore"></param>
    /// <param name="newGrade"></param>
    /// <returns></returns>
    public int CheckAndRegisterRanking(int newScore, GradeType newGrade)
    {
        LoadRankingData();

        int rankIndex = -1;

        // 上から順に比較して今回のスコアが高い場所を探す
        for (int i = 0; i < scores.Length; i++)
        {
            // scores[i] < 0 は未登録状態なので無条件でランクイン
            if (scores[i] < 0 || newScore > scores[i])
            {
                rankIndex = i;
                break;
            }
        }

        // ランクインする場合
        if (rankIndex >= 0)
        {
            // 既存のデータを下位へ押し出す
            for (int i = scores.Length - 1; i > rankIndex; i--)
            {
                scores[i] = scores[i - 1];
                grades[i] = grades[i - 1];
                dates[i] = dates[i - 1];
            }

            // 新しいデータを挿入
            scores[rankIndex] = newScore;
            grades[rankIndex] = newGrade;
            dates[rankIndex] = DateTime.Now.ToString("yyyy/MM/dd");

            // PlayerPrefsに保存
            SaveRankingData();
        }

        // ランクインした順位を返す
        return rankIndex;
    }

    /// <summary>
    /// ランキングデータをPlayerPrefsに保存する
    /// </summary>
    public void SaveRankingData()
    {
        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetInt($"RANKING_SCORE_{i}", scores[i]);
            PlayerPrefs.SetInt($"RANKING_GRADE_{i}", (int)grades[i]);
            PlayerPrefs.SetString($"RANKING_DATE_{i}", dates[i]);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// PlayerPrefsからランキングデータを読み込む
    /// </summary>
    public void LoadRankingData()
    {
        for (int i = 0; i < 5; i++)
        {
            // 初期値がない場合は、スコア-1、成績None、日付空文字
            scores[i] = PlayerPrefs.GetInt($"RANKING_SCORE_{i}", -1);
            grades[i] = (GradeType)PlayerPrefs.GetInt($"RANKING_GRADE_{i}", (int)GradeType.None);
            dates[i] = PlayerPrefs.GetString($"RANKING_DATE_{i}", "");
        }
    }
}
