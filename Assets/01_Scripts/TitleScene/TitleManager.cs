// TitleManager.cs

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

/// <summary>
/// タイトルシーンのUI操作、ウィンドウ開閉、ボタン選択などを管理するクラス。
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("PRESS ANY BUTTON 関連")]
    [SerializeField] private CanvasGroup pressAnyKeyCanvasGroup;
    [SerializeField] private CanvasGroup mainButtonsCanvasGroup;
    [SerializeField] private float pressKeyPulseDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float delayToButtons = 1.0f;

    [Header("タイトルシーン開始時に選択されるボタン")]
    [SerializeField] private GameObject titleFirstSelectionButton;

    [Header("タイトルシーンのメインボタン群")]
    [SerializeField] private Selectable[] titleButtons;

    [Header("ランキングウィンドウ関連")]
    [SerializeField] private GameObject rankingWindow;
    [SerializeField] private GameObject rankingOpenButton;
    [SerializeField] private GameObject rankingCloseButton;

    [Header("システムウィンドウ関連")]
    [SerializeField] private GameObject systemWindow;
    [SerializeField] private GameObject systemOpenButton;
    [SerializeField] private GameObject systemWindowFirstSelectionObject;

    // 内部変数
    private GameObject lastSelectedObject;
    private Tween pressKeyPulseTween;
    private bool isKeyPressed = false;

    void Start()
    {
        AudioManager.PlayBGMTitleScene();

        // メインボタン群を操作不能＆非表示にしておく
        if(mainButtonsCanvasGroup != null)
        {
            mainButtonsCanvasGroup.alpha = 0f;
            mainButtonsCanvasGroup.interactable = false;
            mainButtonsCanvasGroup.blocksRaycasts = false;
        }

        // PRESS ANY BUTTON の点滅を開始
        if(pressAnyKeyCanvasGroup != null)
        {
            pressAnyKeyCanvasGroup.alpha = 1.0f;
            pressKeyPulseTween = pressAnyKeyCanvasGroup.DOFade(0.2f, pressKeyPulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    void Update()
    {
        if(isKeyPressed)
        {
            UIUtils.HandleselectionChangeSE(ref lastSelectedObject);
        }
    }

    void OnDisable()
    {
        pressKeyPulseTween?.Kill();
    }

    #region PRESS ANY BUTTON 処理 ----

    /// <summary>
    /// InputSystemのPressAnyKeyで呼び出される処理
    /// </summary>
    /// <param name="value">入力キー</param>
    public void OnPressAnyKey(InputValue value)
    {
        if(!isKeyPressed && value.isPressed)
        {
            isKeyPressed = true;
            StartCoroutine(SequencePressAnyKey());
        }
    }

    /// <summary>
    /// PRESS ANY BUTTON 押下後の演出シーケンスを実行する処理(コルーチン)
    /// </summary>
    /// <returns>待機時間 IEnumerator</returns>
    private IEnumerator SequencePressAnyKey()
    {
        AudioManager.PlaySEPressAnyButton();

        // PRESS ANY BUTTON の点滅解除し設定時間かけてフェードアウトさせる
        pressKeyPulseTween?.Kill();
        if(pressAnyKeyCanvasGroup != null)
        {
            pressAnyKeyCanvasGroup.DOFade(0f, fadeOutDuration);
        }

        yield return new WaitForSeconds(fadeOutDuration);

        yield return new WaitForSeconds(delayToButtons);

        // メインボタン群を操作可能＆表示させる
        if(mainButtonsCanvasGroup != null)
        {
            mainButtonsCanvasGroup.DOFade(1.0f, 0.5f);
            mainButtonsCanvasGroup.interactable = true;
            mainButtonsCanvasGroup.blocksRaycasts = true;
        }

        lastSelectedObject = UIUtils.SetFocusWithoutSound(titleFirstSelectionButton);
    }

    #endregion // PRESS ANY BUTTON 関係の処理 ----

    #region ボタン関係・ウィンドウ関係の処理 ----

    /// <summary>
    /// メインボタン群の操作可能状態を一括切り替えする処理
    /// </summary>
    /// <param name="interactable">true:操作可能 / false:操作不可能</param>
    private void SetTitleButtonsInteractable(bool interactable)
    {
        if (titleButtons == null) return;

        foreach (var btn in titleButtons)
        {
            if (btn != null)
            {
                btn.interactable = interactable;
            }
        }
    }

    /// <summary>
    /// スタートボタン押下時の処理
    /// </summary>
    public void OnStartButtonPress()
    {
        AudioManager.PlaySESubmit();
        SceneBridge.LoadSceneWithLoading("02_GameScene");
    }

    /// <summary>
    /// ランキングウィンドウを開く処理
    /// </summary>
    public void OpenRankingWindow()
    {
        AudioManager.PlaySESubmit();
        SetTitleButtonsInteractable(false);
        rankingWindow.SetActive(true);

        if (rankingCloseButton != null)
        {
            lastSelectedObject = UIUtils.SetFocusWithoutSound(rankingCloseButton);
        }
        else if(rankingWindow != null)
        {
            // フォールバック
            lastSelectedObject = UIUtils.SetFocusWithoutSound(rankingWindow);
        }
    }

    /// <summary>
    /// ランキングウィンドウを閉じる処理
    /// </summary>
    public void CloseRankingWindow()
    {
        AudioManager.PlaySESubmit();
        SetTitleButtonsInteractable(true);
        rankingWindow.SetActive(false);

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
        SetTitleButtonsInteractable(false);
        systemWindow.SetActive(true);

        // マスター音量のオブジェクトを選択状態にする
        if (systemWindowFirstSelectionObject != null)
        {
            lastSelectedObject = UIUtils.SetFocusWithoutSound(systemWindowFirstSelectionObject);
        }
        else if(systemWindow != null)
        {
            // フォールバック
            lastSelectedObject = UIUtils.SetFocusWithoutSound(systemWindow);
        }
    }

    /// <summary>
    /// システムウィンドウを閉じる処理
    /// </summary>
    public void CloseSystemWindow()
    {
        AudioManager.PlaySESubmit();
        SetTitleButtonsInteractable(true);
        systemWindow.SetActive(false);

        if (systemOpenButton != null)
        {
            lastSelectedObject = UIUtils.SetFocusWithoutSound(systemOpenButton);
        }
    }

    #endregion // ボタン関係・ウィンドウ関係の処理 ----
}