// TitleManager.cs

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// タイトルシーンのUI操作、ウィンドウ開閉、ボタン選択などを管理するクラス。
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("タイトルBGM")]
    [SerializeField] private AudioClip titleBGM;

    [Header("PRESS ANY BUTTON 関連")]
    [SerializeField] private CanvasGroup pressAnyKeyCanvasGroup;
    [SerializeField] private CanvasGroup mainButtonsCanvasGroup;
    [SerializeField] private float pressKeyPulseDuration = 1.0f;  // 「PRESS ANY BUTTON」の点滅時間
    [SerializeField] private float fadeOutDuration = 1.0f;  // 「PRESS ANY BUTTON」が消える時間
    [SerializeField] private float delayToButtons = 1.0f;  // 「PRESS ANY BUTTON」が消えてからスタートボタンなどが出るまでの待ち時間

    [Header("タイトルシーン開始時に選択されるボタン")]
    [SerializeField] private GameObject titleFirstSelectionButton;

    [Header("タイトルシーンのメインボタン群(ウィンドウ表示中は無効化させる)")]
    [SerializeField] private Selectable[] titleButtons;  // スタートボタン, ランキングボタン, システムボタン

    [Header("ランキングウィンドウ関連")]
    [SerializeField] private GameObject rankingWindow;      // ランキングウィンドウ
    [SerializeField] private GameObject rankingOpenButton;  // ランキングウィンドウを「開く」ボタン
    [SerializeField] private GameObject rankingCloseButton;  // ランキングウィンドウの「閉じる」ボタン

    [Header("システムウィンドウ関連")]
    [SerializeField] private GameObject systemWindow;      // システムウィンドウ
    [SerializeField] private GameObject systemOpenButton;  // システムウィンドウを「開く」ボタン
    [SerializeField] private GameObject systemWindowFirstSelectionObject;  // システムウィンドウの始めに選択されるオブジェクト

    // 内部変数リスト
    private GameObject lastSelectedObject;  // 前回選択オブジェクトの記憶
    private Tween pressKeyPulseTween;
    private bool isKeyPressed = false;

    void Start()
    {
        AudioManager.PlayBGM(titleBGM);

        // 最初はメインボタン群を操作不能＆非表示にしておく
        if(mainButtonsCanvasGroup != null)
        {
            mainButtonsCanvasGroup.alpha = 0f;
            mainButtonsCanvasGroup.interactable = false;
            mainButtonsCanvasGroup.blocksRaycasts = false;
        }

        // 「PRESS ANY BUTTON」の点滅を開始
        if(pressAnyKeyCanvasGroup != null)
        {
            pressAnyKeyCanvasGroup.alpha = 1.0f;
            pressKeyPulseTween = pressAnyKeyCanvasGroup.DOFade(0.2f, pressKeyPulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // 選択ボタンの設定
        //lastSelectedObject = UIUtils.SetFocusWithoutSound(titleFirstSelectionButton);
    }

    void Update()
    {
        // 「PRESS ANY BUTTON」の入力待ち
        if(!isKeyPressed)
        {
            if(Input.anyKeyDown)
            {
                isKeyPressed = true;
                StartCoroutine(SequencePressAnyKey());
            }
            return;
        }

        // キーボードやゲームパッド操作によるボタン移動を監視し移動音を鳴らす
        UIUtils.HandleselectionChangeSE(ref lastSelectedObject);
    }

    private IEnumerator SequencePressAnyKey()
    {
        AudioManager.PlaySESubmit();

        // 「PRESS ANY BUTTON」の点滅解除し1秒かけて消滅させる
        pressKeyPulseTween?.Kill();
        if(pressAnyKeyCanvasGroup != null)
        {
            pressAnyKeyCanvasGroup.DOFade(0f, fadeOutDuration);
        }

        // 1秒間待機
        yield return new WaitForSeconds(fadeOutDuration);

        // 更に1秒間待機
        yield return new WaitForSeconds(delayToButtons);

        // メインボタン群を操作可能＆表示させる
        if(mainButtonsCanvasGroup != null)
        {
            mainButtonsCanvasGroup.DOFade(1.0f, 0.5f);
            mainButtonsCanvasGroup.interactable = true;
            mainButtonsCanvasGroup.blocksRaycasts = true;
        }

        // 最初にフォーカスするボタンの設定
        lastSelectedObject = UIUtils.SetFocusWithoutSound(titleFirstSelectionButton);
    }

    void OnDisable()
    {
        pressKeyPulseTween?.Kill();
    }

    #region スタートボタン, ウィンドウ表示・非表示処理 ----

    /// <summary>
    /// スタートボタン押下時の処理
    /// </summary>
    public void OnStartButtonPress()
    {
        AudioManager.PlaySESubmit();
        SceneManager.LoadScene("02_GameScene");
    }

    /// <summary>
    /// ランキングウィンドウを開く処理
    /// </summary>
    public void OpenRankingWindow()
    {
        AudioManager.PlaySESubmit();
        SetTitleButtonsInteractable(false);  // タイトルメインボタン群の操作を無効化
        rankingWindow.SetActive(true);

        // 閉じるボタンを選択状態にする
        if(rankingCloseButton != null)
        {
            lastSelectedObject = UIUtils.SetFocusWithoutSound(rankingCloseButton);
        }
    }

    /// <summary>
    /// ランキングウィンドウを閉じる処理
    /// </summary>
    public void CloseRankingWindow()
    {
        AudioManager.PlaySESubmit();
        SetTitleButtonsInteractable(true);  // タイトルメインボタン群の操作を有効化
        rankingWindow.SetActive(false);

        // ランキングボタンを選択状態にする
        if(rankingOpenButton != null)
        {
            lastSelectedObject = UIUtils.SetFocusWithoutSound(rankingOpenButton);
        }
    }

    /// <summary>
    /// システムウィンドウを開く処理
    /// </summary>
    public void OpenSystemWindow()
    {
        AudioManager.PlaySESubmit();
        SetTitleButtonsInteractable(false);  // タイトルメインボタン群の操作を無効化
        systemWindow.SetActive(true);

        // マスター音量のオブジェクトを選択状態にする
        if (systemWindowFirstSelectionObject != null)
        {
            lastSelectedObject = UIUtils.SetFocusWithoutSound(systemWindowFirstSelectionObject);
        }
        else if(systemWindow != null)
        {
            // フォールバック：未設定時はsystemWindow自体をフォーカス
            lastSelectedObject = UIUtils.SetFocusWithoutSound(systemWindow);
        }
    }

    /// <summary>
    /// システムウィンドウを閉じる処理
    /// </summary>
    public void CloseSystemWindow()
    {
        AudioManager.PlaySESubmit();
        SetTitleButtonsInteractable(true);  // タイトルメインボタン群の操作を有効化
        systemWindow.SetActive(false);

        // システムボタンを選択状態にする
        if(systemOpenButton != null)
        {
            lastSelectedObject = UIUtils.SetFocusWithoutSound(systemOpenButton);
        }
    }

    #endregion // スタートボタン, ウィンドウ表示・非表示処理 ----

    /// <summary>
    /// タイトルのメインボタン群の操作可能状態(有効/無効)を一括切り替えする
    /// </summary>
    /// <param name="interactable">true:操作可能 / false:操作不可能</param>
    private void SetTitleButtonsInteractable(bool interactable)
    {
        if (titleButtons == null) return;

        foreach(var btn in titleButtons)
        {
            // ボタンが空でない場合、ボタンの操作可能状態を切り替える。
            if(btn != null)
            {
                btn.interactable = interactable;
            }
        }
    }
}
