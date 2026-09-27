// RankingWindowManager

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ランキングウィンドウの内容を管理するクラス
/// </summary>
public class RankingWindowManager : MonoBehaviour
{
    [Header("各成績画像のデータ設定")]
    [SerializeField] private GradeImageData[] gradeImageDates;

    [Header("各成績画像表示先UI_Image")]
    [SerializeField] private Image[] imageGrades = new Image[5];

    [Header("透明画像(初期化・該当なし用)")]
    [SerializeField] private Sprite clearSprite;

    [Header("各順位の成績(初期値:None)")]
    [SerializeField]
    private GradeType[] grades = new GradeType[5]
    {
        GradeType.None,
        GradeType.None,
        GradeType.None,
        GradeType.None,
        GradeType.None
    };

    [Header("各スコア表示用UI")]
    [SerializeField] private TextMeshProUGUI[] txtScores = new TextMeshProUGUI[5];

    [Header("各スコア値(初期値:-1)")]
    [SerializeField] private int[] scores = new int[5] { -1, -1, -1, -1, -1 };

    [Header("各日付表示用UI")]
    [SerializeField] private TextMeshProUGUI[] txtDates = new TextMeshProUGUI[5];

    [Header("各日付データ(初期値:空文字)")]
    [SerializeField] private string[] dates = new string[5] { "", "", "", "", "" };

    void Start()
    {
        //ランキングウィンドウの成績画像, スコア, 日付を更新する
        UpdateRankingUI();
    }

    /// <summary>
    /// 各成績画像, 各スコア値, 各日付をUIに入れる処理
    /// </summary>
    public void UpdateRankingUI()
    {
        for(int i = 0; i < txtScores.Length; i++)
        {
            // 各成績画像を更新する
            if (imageGrades[i] != null)
            {
                UpdateGradeImage(imageGrades[i], grades[i]);
            }

            // 各スコアテキストを更新する
            if (txtScores[i] != null)
            {
                if(scores[i] < 0)
                {
                    txtScores[i].text = "----";
                }
                else
                {
                    txtScores[i].text = scores[i].ToString("N0") + "pt";
                }
            }

            // 各日付テキストを更新する
            if(txtDates != null)
            {
                if (string.IsNullOrEmpty(dates[i]))
                {
                    txtDates[i].text = "----/--/--";
                }
                else
                {
                    txtDates[i].text = dates[i];
                }
            }
        }
    }

    #region 画像関係処理まとめ

    /// <summary>
    /// UI_Imageの成績種類に対応する画像データをセットする処理
    /// </summary>
    /// <param name="targetImage">指定した成績画像</param>
    /// <param name="type">成績種類</param>
    private void UpdateGradeImage(Image targetImage, GradeType type)
    {
        if (targetImage == null) return;

        // 成績種類に対応した画像を見つけてくる
        Sprite foundSprite = GetGradeSprite(type);

        if(foundSprite != null)
        {
            // 見つけてきた画像データを指定したUI_Imageに入れUI表示をONにする
            targetImage.sprite = foundSprite;
            targetImage.enabled = true;
        }
        else
        {
            // 透明画像の場合の処理
            targetImage.sprite = clearSprite;
            targetImage.enabled = (clearSprite != null);
        }
    }

    /// <summary>
    /// 成績種類に対応した画像を取得する処理
    /// </summary>
    /// <param name="type">成績種類</param>
    /// <returns>対応する画像データ Sprite</returns>
    private Sprite GetGradeSprite(GradeType type)
    {
        if (gradeImageDates == null) return clearSprite;

        // 成績種類に応じた画像データを返す
        foreach (var data in gradeImageDates)
        {
            if(data.gradeType == type)
            {
                return data.imageSprite;
            }
        }

        // 見つからない場合は透明画像を返す
        return clearSprite;
    }

    #endregion // 画像関係処理まとめ

    #region 一括更新用の処理まとめ ----

    /// <summary>
    /// 各成績を一括更新する処理
    /// </summary>
    /// <param name="newGrades">更新用の成績(配列)</param>
    public void SetGrades(GradeType[] newGrades)
    {
        if (newGrades == null || newGrades.Length != grades.Length) return;

        // 各成績を更新する
        for(int i = 0; i < grades.Length;i++ )
        {
            grades[i] = newGrades[i];
        }

        UpdateRankingUI();
    }

    /// <summary>
    /// 各スコア値を一括更新する処理
    /// </summary>
    /// <param name="newScores">更新用のスコア(配列)</param>
    public void SetScores(int[] newScores)
    {
        if (newScores == null || newScores.Length != scores.Length) return;

        // 各スコア値を更新する
        for (int i = 0; i < scores.Length; i++)
        {
            scores[i] = newScores[i];
        }

        UpdateRankingUI();
    }

    /// <summary>
    /// 各日付を一括更新する処理
    /// </summary>
    /// <param name="newDates">更新用の各日付(配列)</param>
    public void SetDates(string[] newDates)
    {
        if (newDates == null || newDates.Length != dates.Length) return;

        // 各日付を更新する
        for(int i = 0; i < dates.Length; i++)
        {
            dates[i] = newDates[i];
        }

        UpdateRankingUI();
    }

    /// <summary>
    /// 各成績, 各スコア, 各日付を同時に一括更新する処理
    /// </summary>
    /// <param name="newGrades">更新用の各成績(配列)</param>
    /// <param name="newScores">更新用の各スコア(配列)</param>
    /// <param name="newDates">更新用の各日付(配列)</param>
    public void SetRankingDate(GradeType[] newGrades, int[] newScores, string[] newDates)
    {
        // ガード節と各成績を更新する
        if (newGrades != null && newGrades.Length == grades.Length)
        {
            for (int i = 0; i < grades.Length; i++)
            {
                grades[i] = newGrades[i];
            }
        }

        // ガード節と各スコア値を更新する
        if (newScores != null && newScores.Length == scores.Length)
        {
            for(int i = 0; i < scores.Length; i++)
            {
                scores[i] = newScores[i];
            }
        }

        // ガード節と各日付を更新する
        if(newDates != null && newDates.Length == dates.Length)
        {
            for(int i = 0; i < dates.Length; i++)
            {
                dates[i] = newDates[i];
            }
        }

        UpdateRankingUI();
    }

    #endregion // 一括更新用の処理まとめ
}
