// RankingWindowManager

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ランキングウィンドウの内容を管理するクラス
/// </summary>
public class RankingWindowManager : MonoBehaviour
{
    // 各成績のX座標位置調整用(構造体)
    [System.Serializable]
    public struct GradeOffset
    {
        public GradeType gradeType;
        [Tooltip("基準位置からX移動値")] public float xOffset;
        [Tooltip("基準位置からY移動値")] public float yOffset;
    }

    [Header("各成績のX座標オフセット設定")]
    [SerializeField] private GradeOffset[] gradeOffsets;

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

    // 各成績画像オブジェクトの初期位置を保存しておく配列
    private Vector2[] defaultImagePositions = new Vector2[5];
    private bool isInitializedPositions = false;

    void Start()
    {
        CacheDefalutPositions();

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

            // 画像データに対しての座標位置オフセットを適用する
            ApplyGradeOffset(targetImage, type);
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

    /// <summary>
    /// 各成績画像オブジェクトの本来位置をキャッシュする処理
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
    /// <param name="targetImage"></param>
    /// <param name="type"></param>
    private void ApplyGradeOffset(Image targetImage, GradeType type)
    {
        RectTransform rectTrans = targetImage.GetComponent<RectTransform>();
        if(rectTrans == null) return;

        // 何番目のImageか特定する
        int index = -1;
        for(int i = 0; i < imageGrades.Length; i++)
        {
            if (imageGrades[i] == targetImage)
            {
                index = i;
                break;
            }
        }
        if (index < 0) return;

        // 設定されたオフセット値を探す
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

        // 現在のローカルポジションを取得し座標にオフセットを加算する
        Vector2 basePos = defaultImagePositions[index];
        rectTrans.anchoredPosition = new Vector2(basePos.x + addX, basePos.y + addY);
    }

    #endregion // 画像関係処理まとめ
}
