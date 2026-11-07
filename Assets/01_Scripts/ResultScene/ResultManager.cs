// ResultManager.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

/// <summary>
/// スコア閾値と対応する成績種類データ
/// </summary>
[Serializable]
public struct GradeThresholdData
{
    [Header("スコア閾値")]
    public int thresholdScore;
    [Header("対応する成績種類")]
    public GradeType gradeType;
}

/// <summary>
/// リザルトシーン全体を管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    [Header("ランキング管理への参照")]
    [SerializeField] private RankingWindowManager rankingWindowManager;

    [Header("スコア表示用UI")]
    [SerializeField] private TextMeshProUGUI ResultScore;

    [Header("スコアのカウントアップ演出設定")]
    [SerializeField] private float countUpDuration = 1.25f;
    [SerializeField] private Ease countUpEase = Ease.OutExpo;

    [Header("各成績種類のスコア閾値設定")]
    [SerializeField]
    private GradeThresholdData[] gradeThresholds = new GradeThresholdData[]
    {
        new GradeThresholdData {thresholdScore = 100000, gradeType = GradeType.MVPSlugger },
        new GradeThresholdData {thresholdScore = 80000, gradeType = GradeType.GoldSlugger },
        new GradeThresholdData {thresholdScore = 60000, gradeType = GradeType.SliverSlugger },
        new GradeThresholdData {thresholdScore = 40000, gradeType = GradeType.MainSlugger },
        new GradeThresholdData {thresholdScore = 20000, gradeType = GradeType.Moudasyo },
        new GradeThresholdData {thresholdScore = 0, gradeType = GradeType.LightHitter }
    };

    [Header("各成績画像のデータ設定")]
    [SerializeField] private GradeImageData[] gradeImageDatas;

    [Header("成績画像表示用UI_Image")]
    [SerializeField] private Image imageGrade;

    [Header("透明画像(初期化・該当なし用)")]
    [SerializeField] private Sprite clearSprite;

    [Header("ボタン群のオブジェクト")]
    [SerializeField] private CanvasGroup buttonGroupCanvasGroup;

    [Header("リザルトシーン開始時に選択されるボタン")]
    [SerializeField] private GameObject resultFirstSelectionButton;

    [Header("デバッグ用設定")]
    [SerializeField] private bool isDebugMode = false;
    [SerializeField] private int debugScore = 0;

    // 内部変数
    private GameObject lastSelectedObject;
    private int cachedFinalScore;
    private Tween countUpTween;
    private Sequence resultSequence;

    void Start()
    {
        AudioManager.StopBGM();
        InitializeButtonsUI();

        if(!isDebugMode)
        {
            cachedFinalScore = PlayerPrefs.GetInt("FINAL_SCORE", 0);
        }
        else
        {
            cachedFinalScore = debugScore;
        }

        PlayResultSequence(cachedFinalScore);
    }

    void Update()
    {
        UIUtils.HandleselectionChangeSE(ref lastSelectedObject);
    }

    #region スコア・成績表示処理 ----

    /// <summary>
    /// スコアUI表示を更新する処理
    /// </summary>
    /// <param name="score">スコア値</param>
    private void UpdateScoreUI(int score)
    {
        if (ResultScore == null) return;

        ResultScore.text = string.Format("{0:N0}<size=80%><color=#888888>pt</color></size>", score);
    }

    /// <summary>
    /// 最終スコアから成績画像をUI_Imageにセットする処理
    /// </summary>
    /// <param name="score">最終スコア</param>
    private void UpdateGradeImageByScore(int score)
    {
        Sprite targetSprite;
        GradeType achievedGrade = GetGradeTypeByScore(score);

        targetSprite = GetSpriteByGradeType(achievedGrade);

        if(targetSprite != null && targetSprite != clearSprite)
        {
            imageGrade.sprite = targetSprite;
            imageGrade.enabled = true;
        }
        else
        {
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
        if (type == GradeType.None || gradeImageDatas == null) return clearSprite;

        foreach(var data in gradeImageDatas)
        {
            if(data.gradeType == type)
            {
                return data.imageSprite;
            }
        }

        return clearSprite;
    }

    /// <summary>
    /// スコアからGradeTypeを取得する処理
    /// </summary>
    /// <param name="score">スコア値</param>
    /// <returns></returns>
    private GradeType GetGradeTypeByScore(int score)
    {
        if(gradeThresholds == null || gradeThresholds.Length == 0) return GradeType.None;

        foreach(var threshold in gradeThresholds)
        {
            if(score >= threshold.thresholdScore)
            {
                return threshold.gradeType;
            }
        }

        return GradeType.None;
    }

    #endregion // スコア・成績表示処理 ----

    #region 各ボタンの処理 ----

    /// <summary>
    /// ボタン群を初期状態(透明・操作不能)にする処理
    /// </summary>
    private void InitializeButtonsUI()
    {
        if (buttonGroupCanvasGroup == null) return;

        buttonGroupCanvasGroup.alpha = 0f;
        buttonGroupCanvasGroup.interactable = false;
        buttonGroupCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// ボタン群をフェードインさせ操作可能にする処理
    /// </summary>
    private void ShowButtons()
    {
        if(buttonGroupCanvasGroup != null)
        {
            buttonGroupCanvasGroup.DOFade(1f, 0.4f).OnComplete(() =>
            {
                buttonGroupCanvasGroup.interactable = true;
                buttonGroupCanvasGroup.blocksRaycasts = true;

                // ボタンが活性化してからフォーカスさせる
                lastSelectedObject = UIUtils.SetFocusWithoutSound(resultFirstSelectionButton);
            });
        }
        else
        {
            // フォールバック
            lastSelectedObject = UIUtils.SetFocusWithoutSound(resultFirstSelectionButton);
        }
    }

    /// <summary>
    /// 「もう一回！」ボタンの処理
    /// ボタン押下時にゲームシーンへ移動する
    /// </summary>
    public void OnOneMoreTimeButtonPress()
    {
        KillAllTweens();
        AudioManager.PlaySESubmit();
        SceneBridge.LoadSceneWithLoading("02_GameScene");
    }

    /// <summary>
    /// 「タイトル移動」ボタンの処理
    /// ボタン押下時にタイトルシーンへ移動する
    /// </summary>
    public void OnTitleMoveButtonPress()
    {
        KillAllTweens();
        AudioManager.PlaySESubmit();
        SceneBridge.LoadSceneWithLoading("01_TitleScene");
    }

    #endregion // 各ボタンの処理 ----

    #region その他処理 ----

    /// <summary>
    /// 一連のリザルト演出(スコアカウントアップ → 成績表示 → ボタン表示)の処理
    /// </summary>
    /// <param name="targetScore">最終スコア</param>
    private void PlayResultSequence(int targetScore)
    {
        // 成績画像を非表示する
        if (imageGrade != null)
        {
            imageGrade.sprite = clearSprite;
            imageGrade.enabled = (clearSprite != null);
            imageGrade.transform.localScale = Vector3.zero;  // 拡大率を0にする
        }

        KillAllTweens();

        // スコアが0以上だったらカウントアップSEを鳴らす
        if (targetScore > gradeThresholds[5].thresholdScore)
        {
            AudioManager.PlaySECountUp();
        }

        // ---- 【ステップ1】スコアのカウントアップ演出
        countUpTween = DOVirtual.Float(0f, targetScore, countUpDuration, value =>
        {
            UpdateScoreUI(Mathf.FloorToInt(value));
        })
        .SetEase(countUpEase)
        .OnComplete(() =>
        {
            UpdateScoreUI(targetScore);

            // カウントアップが完了したタイミングでランキングの判定・保存を行う
            if (RankingDataStore.Instance != null)
            {
                // スコアから成績を判別し、ランキング更新する処理を呼ぶのか判断する。
                GradeType achievedGrade = GetGradeTypeByScore(targetScore);

                int registeredRank = RankingDataStore.Instance.CheckAndRegisterRanking(targetScore, achievedGrade);
                if (registeredRank >= 0)
                {
                    Debug.Log($"{registeredRank + 1}位にランクインしました！");
                }
            }
        });

        resultSequence = DOTween.Sequence();

        resultSequence.Append(countUpTween);

        // ---- 【ステップ2】成績画像の表示
        resultSequence.AppendCallback(() =>
        {
            // 成績によってグレードSEを変える
            if (targetScore >= gradeThresholds[2].thresholdScore)
            {
                AudioManager.PlaySEHighGrade();
            }
            else if (targetScore >= gradeThresholds[4].thresholdScore)
            {
                AudioManager.PlaySEMiddleGrade();
            }
            else
            {
                AudioManager.PlaySELowGrade();
            }

            UpdateGradeImageByScore(targetScore); 
        });

        if (imageGrade != null)
        {
            // 画像が少し跳ねるアニメーション
            resultSequence.Append(imageGrade.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
        }

        resultSequence.AppendCallback(() =>
        {
            if (targetScore >= 60000)
            {
                AudioManager.PlaySECheer();
            }
            else if (targetScore >= 20000)
            {
                AudioManager.PlaySEApplause();
            }
        });

        resultSequence.AppendInterval(0.2f);

        // ---- 【ステップ3】ボタン群のフェードインとフォーカス
        resultSequence.AppendCallback(() =>
        {
            ShowButtons();
        });
    }

    /// <summary>
    /// 全てのTweenを破棄する処理
    /// </summary>
    private void KillAllTweens()
    {
        countUpTween?.Kill();
        resultSequence?.Kill();
    }

    /// <summary>
    /// ResultManager破棄時に全てのTweenを破棄する処理
    /// </summary>
    private void OnDestroy()
    {
        KillAllTweens();
    }

    #if UNITY_EDITOR

    /// <summary>
    /// Inspectorでデバッグ用スコア値を変更した瞬間に自動実行される処理
    /// </summary>
    private void OnValidate()
    {
        // ゲーム中かつデバッグモードONの場合、リアルタイムでスコアを反映する。
        if(Application.isPlaying && isDebugMode)
        {
            UpdateScoreUI(debugScore);
            UpdateGradeImageByScore(debugScore);
        }
    }

    #endif

    #endregion // その他処理 ----
}