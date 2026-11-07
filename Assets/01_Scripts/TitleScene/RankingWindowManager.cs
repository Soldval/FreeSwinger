// RankingWindowManager

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// ランキングウィンドウの内容を管理するクラス
/// </summary>
public class RankingWindowManager : MonoBehaviour
{
    // 各成績のX・Y座標位置調整用(構造体)
    [System.Serializable]
    public struct GradeOffset
    {
        public GradeType gradeType;
        [Tooltip("基準位置からX移動値")] public float xOffset;
        [Tooltip("基準位置からY移動値")] public float yOffset;
    }

    [Header("各成績のX・Y座標オフセット設定")]
    [SerializeField] private GradeOffset[] gradeOffsets;

    [Header("各成績画像のデータ設定")]
    [SerializeField] private GradeImageData[] gradeImageDates;

    [Header("各成績画像表示先UI_Image")]
    [SerializeField] private Image[] imageGrades;

    [Header("透明画像(初期化・該当なし用)")]
    [SerializeField] private Sprite clearSprite;

    [Header("各順位の成績(初期値:None)")]
    [SerializeField]
    private GradeType[] grades;

    [Header("各スコア表示用UI")]
    [SerializeField] private TextMeshProUGUI[] txtScores;

    [Header("各スコア値(初期値:-1)")]
    [SerializeField] private int[] scores;

    [Header("各日付表示用UI")]
    [SerializeField] private TextMeshProUGUI[] txtDates;

    [Header("各日付データ(初期値:空文字)")]
    [SerializeField] private string[] dates;

    // 内部変数
    private Vector2[] defaultImagePositions;
    private bool isInitializedPositions = false;

    void Start()
    {
        if(RankingDataStore.Instance != null)
        {
            // rankingDataLengthに合わせて各配列を初期化する
            int length = RankingDataStore.Instance.rankingDataLength;
            imageGrades = ResizeArray(imageGrades, length);
            grades = new GradeType[length];
            txtScores = ResizeArray(txtScores, length);
            scores = new int[length];
            txtDates = ResizeArray(txtDates, length);
            dates = new string[length];
            defaultImagePositions = new Vector2[length];

            // 初期値を入れる
            for(int i = 0; i < length; i++)
            {
                grades[i] = GradeType.None;
                scores[i] = -1;
                dates[i] = "";
            }

            // RankingDataStoreから各データをロードし配列に入れる
            RankingDataStore.Instance.LoadRankingData();
            for(int i = 0; i < length; i++)
            {
                scores[i] = RankingDataStore.Instance.scores[i];
                grades[i] = RankingDataStore.Instance.grades[i];
                dates[i] = RankingDataStore.Instance.dates[i];
            }
        }

        CacheDefalutPositions();

        UpdateRankingUI();
    }

    /// <summary>
    /// 配列長さを安全に変更するためのヘルパー処理
    /// </summary>
    /// <typeparam name="T">配列のデータ型</typeparam>
    /// <param name="oldArray">更新したい配列</param>
    /// <param name="newSize">更新配列サイズ</param>
    /// <returns>更新された配列 T[]</returns>
    private T[] ResizeArray<T>(T[] oldArray, int newSize)
    {
        if (oldArray != null && oldArray.Length == newSize) return oldArray;

        T[] newArray = new T[newSize];

        // 古い配列を新しい配列にコピーする
        if(oldArray != null)
        {
            int copyLength = Math.Min(oldArray.Length, newSize);
            Array.Copy(oldArray, newArray, copyLength);
        }
        return newArray;
    }

    /// <summary>
    /// 各成績画像, 各スコア値, 各日付をUIに入れる処理
    /// </summary>
    public void UpdateRankingUI()
    {
        if (txtScores == null) return;

        for(int i = 0; i < txtScores.Length; i++)
        {
            // 各成績画像更新
            if (imageGrades[i] != null && i < imageGrades.Length && imageGrades[i] != null)
            {
                UpdateGradeImage(imageGrades[i], grades[i]);
            }

            // 各スコアテキスト更新
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

            // 各日付テキスト更新
            if(txtDates != null && i < txtDates.Length && txtDates[i] != null)
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

    #region 画像関係の処理 ----

    /// <summary>
    /// UI_Imageの成績種類に対応する画像データをセットする処理
    /// </summary>
    /// <param name="targetImage">指定した成績画像</param>
    /// <param name="type">成績種類</param>
    private void UpdateGradeImage(Image targetImage, GradeType type)
    {
        if (targetImage == null) return;

        Sprite foundSprite = GetGradeSprite(type);

        if(foundSprite != null)
        {
            targetImage.sprite = foundSprite;
            targetImage.enabled = true;
            ApplyGradeOffset(targetImage, type);
        }
        else
        {
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

        foreach (var data in gradeImageDates)
        {
            if(data.gradeType == type)
            {
                return data.imageSprite;
            }
        }

        return clearSprite;
    }

    /// <summary>
    /// 各成績画像オブジェクトの本来位置を一時保存する処理
    /// </summary>
    private void CacheDefalutPositions()
    {
        if (isInitializedPositions) return;

        for(int i = 0; i < imageGrades.Length; i++)
        {
            if (imageGrades[i] != null)
            {
                defaultImagePositions[i] = imageGrades[i].GetComponent<RectTransform>().anchoredPosition;
            }
        }

        isInitializedPositions = true;
    }

    /// <summary>
    /// 成績種類に対応した座標位置オフセットを適用する処理
    /// </summary>
    /// <param name="targetImage">指定した成績画像</param>
    /// <param name="type">成績種類</param>
    private void ApplyGradeOffset(Image targetImage, GradeType type)
    {
        RectTransform rectTrans = targetImage.GetComponent<RectTransform>();
        if(rectTrans == null) return;

        // ---- 何番目のImageか特定する
        int index = -1;
        for(int i = 0; i < imageGrades.Length; i++)
        {
            if (imageGrades[i] == targetImage)
            {
                index = i;
                break;
            }
        }
        if (index < 0 || index >= defaultImagePositions.Length) return;

        // ---- 設定されたオフセット値を探す
        float addX = 0f;
        float addY = 0f;
        if(gradeOffsets != null)
        {
            foreach(var offset in gradeOffsets)
            {
                if(offset.gradeType == type)
                {
                    addX = offset.xOffset;
                    addY = offset.yOffset;
                    break;
                }
            }
        }

        // ---- 現在のローカルポジションを取得し座標にオフセットを加算する
        Vector2 basePos = defaultImagePositions[index];
        rectTrans.anchoredPosition = new Vector2(basePos.x + addX, basePos.y + addY);
    }

    #endregion // 画像関係の処理 ----
}
