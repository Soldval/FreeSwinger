// ResultManager.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// リザルトシーン全体を管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    [Header("スコア表示用UI")]
    [SerializeField] private TextMeshProUGUI ResultScore;

    [Header("各成績画像のデータ設定")]
    [SerializeField] private GradeImageData[] gradeImageDatas;

    [Header("成績画像表示用UI_Image")]
    [SerializeField] private Image imageGrade;

    [Header("透明画像(初期化・該当なし用)")]
    [SerializeField] private Sprite clearSprite;

    [Header("リザルトシーン開始時に選択されるボタン")]
    [SerializeField] private GameObject resultFirstSelectionButton;

    [Header("デバッグ用設定")]
    [SerializeField] private bool isDebugMode = false;  // デバッグモード
    [SerializeField] private int debugScore = 0; // デバッグ用スコア

    // 前回選択オブジェクトの記憶
    private GameObject lastSelectedObject;

    void Start()
    {
        AudioManager.StopBGM();  // BGMを停止

        int finalScore;  // 最終スコア

        if(!isDebugMode)
        {
            // 通常時の処理
            finalScore = PlayerPrefs.GetInt("FINAL_SCORE", 0);
        }
        else
        {
            // デバッグモード時の処理
            finalScore = debugScore;
        }

        // 最終スコア表示と成績画像表示
        SetResultData(finalScore);

        // 選択ボタンの設定
        lastSelectedObject = UIUtils.SetFocusWithoutSound(resultFirstSelectionButton);
    }

    void Update()
    {
        // キーボードやゲームパッド操作によるボタン移動を監視し移動音を鳴らす
        UIUtils.HandleselectionChangeSE(ref lastSelectedObject);
    }

    #region スコア・成績表示処理 ----

    /// <summary>
    /// スコアと成績画像を表示する処理
    /// </summary>
    /// <param name="score">最終スコア</param>
    public void SetResultData(int score)
    {
        // スコア表示を更新
        if(ResultScore != null)
        {
            // 「スコア + pt(少し小さい)」 を表示 
            ResultScore.text = string.Format("{0:N0}<size=80%><color=#888888>pt</color></size>", score);
        }

        // 成績種類に応じた画像データを更新
        UpdateGradeImageByScore(score);
    }

    /// <summary>
    /// 最終スコアによる成績画像をUI_Imageにセットする処理
    /// </summary>
    /// <param name="score">最終スコア</param>
    private void UpdateGradeImageByScore(int score)
    {
        Sprite targetSprite;  // 画像データの一時的な保存用

        // 最終スコアに応じた画像データを一時保存に入れる
        if(score >= 100000)
        {
            targetSprite = GetSpriteByGradeType(GradeType.MVPSlugger);
        }
        else if(score >= 80000)
        {
            targetSprite = GetSpriteByGradeType(GradeType.GoldSlugger);
        }
        else if(score >= 60000)
        {
            targetSprite = GetSpriteByGradeType(GradeType.SliverSlugger);
        }
        else if(score >= 40000)
        {
            targetSprite = GetSpriteByGradeType(GradeType.MainSlugger);
        }
        else if(score >= 20000)
        {
            targetSprite = GetSpriteByGradeType(GradeType.Moudasyo);
        }
        else if(score >= 0)
        {
            targetSprite = GetSpriteByGradeType(GradeType.LightHitter);
        }
        else
        {
            targetSprite = GetSpriteByGradeType(GradeType.None);
        }

        
        if(targetSprite != null && targetSprite != clearSprite)
        {
            // 見つけてきた画像データを画像表示用UIに入れUI表示をONにする
            imageGrade.sprite = targetSprite;
            imageGrade.enabled = true;
        }
        else
        {
            // 透明画像の場合の処理
            imageGrade.sprite = clearSprite;
            imageGrade.enabled = (clearSprite != null);
        }
    }

    /// <summary>
    /// 成績種類に応じた画像データを取得する処理
    /// </summary>
    /// <param name="type">成績種類</param>
    /// <returns>対応する画像データ Sprite</returns>
    private Sprite GetSpriteByGradeType(GradeType type)
    {
        // ガード節
        if (type == GradeType.None || gradeImageDatas == null) return clearSprite;

        // 成績種類に応じた画像データを返す
        foreach(var data in gradeImageDatas)
        {
            if(data.gradeType == type)
            {
                return data.imageSprite;
            }
        }

        // 見つからない場合は透明画像を返す
        return clearSprite;
    }

    #endregion // スコア・成績表示処理 ----

    #region 各ボタンの処理 ----

    /// <summary>
    /// 「もう一回！」ボタンの処理
    /// ボタン押下時にゲームシーンへ移動する
    /// </summary>
    public void OnOneMoreTimeButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        SceneManager.LoadScene("02_GameScene");  // ゲームシーンへ移動
    }

    /// <summary>
    /// 「タイトル移動」ボタンの処理
    /// ボタン押下時にタイトルシーンへ移動する
    /// </summary>
    public void OnTitleMoveButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        SceneManager.LoadScene("01_TitleScene");  // タイトルシーンへ移動
    }

    #endregion // 各ボタンの処理 ----

    #if UNITY_EDITOR

    /// <summary>
    /// Inspectorでデバッグ用スコア値を変更した瞬間に自動実行される処理
    /// </summary>
    private void OnValidate()
    {
        // ゲーム中かつデバッグモードONの場合、リアルタイムでスコアを反映する。
        if(Application.isPlaying && isDebugMode)
        {
            SetResultData(debugScore);
        }
    }

    #endif
}