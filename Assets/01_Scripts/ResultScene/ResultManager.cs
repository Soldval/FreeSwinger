// ResultManager.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// リザルトシーン全体を管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    [Header("スコア表示用UI")]
    [SerializeField] private TextMeshProUGUI ResultScore;

    [Header("スコアのカウントアップ演出設定")]
    [SerializeField] private float countUpDuration = 1.25f;  // カウントアップにかかる時間
    [SerializeField] private Ease countUpEase = Ease.OutExpo;  // 減速アニメーションのイージング

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
    private Tween countUpTween;
    private Sequence resultSequence;

    void Start()
    {
        int finalScore;

        AudioManager.StopBGM();

        InitializeButtonsUI();

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

        PlayResultSequence(finalScore);
    }

    void Update()
    {
        // キーボードやゲームパッド操作によるボタン移動を監視し移動音を鳴らす
        UIUtils.HandleselectionChangeSE(ref lastSelectedObject);
    }

    #region スコア・成績表示処理 ----

    /// <summary>
    /// スコアのUI表示を更新する処理
    /// </summary>
    /// <param name="score">スコア値</param>
    private void UpdateScoreUI(int score)
    {
        if (ResultScore == null) return;

        ResultScore.text = string.Format("{0:N0}<size=80%><color=#888888>pt</color></size>", score);
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

        if (targetScore > 0)
        {
            AudioManager.PlaySECountUp();  // カウントアップ効果音を再生
        }

        // 【ステップ1】スコアのカウントアップ演出
        countUpTween = DOVirtual.Float(0f, targetScore, countUpDuration, value =>
        {
            UpdateScoreUI(Mathf.FloorToInt(value));
        })
        .SetEase(countUpEase)
        .OnComplete(() =>
        {
            UpdateScoreUI(targetScore);
        });

        // シーケンス演出の構築
        resultSequence = DOTween.Sequence();

        // カウントアップ完了まで待つ
        resultSequence.Append(countUpTween);

        // 【ステップ2】成績画像の表示
        resultSequence.AppendCallback(() =>
        {
            if (targetScore >= 60000)
            {
                AudioManager.PlaySEHighGrade();
            }
            else if (targetScore >= 20000)
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

        // 少しだけ余韻
        resultSequence.AppendInterval(0.2f);

        // 【ステップ3】ボタン群のフェードインとフォーカス
        resultSequence.AppendCallback(() =>
        {
            ShowButtons();
        });
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
            // CanvasGroupがない場合のフォールバック
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
    /// 全てのTweenを破棄する処理
    /// </summary>
    private void KillAllTweens()
    {
        countUpTween?.Kill();
        resultSequence?.Kill();
    }

    /// <summary>
    /// このオブジェクト破棄の時、全てのTweenを破棄する処理。
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