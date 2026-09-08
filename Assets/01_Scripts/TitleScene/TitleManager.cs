// TitleManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// タイトルシーンのUI操作、ウィンドウ開閉、ボタン選択などを管理するクラス。
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("タイトルBGM")]
    [SerializeField] private AudioClip titleBGM;

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
    [SerializeField] private GameObject systemMasterSliderObject;  // システムウィンドウのマスター音量オブジェクト

    // 前回選択オブジェクトの記憶
    private GameObject lastSelectedObject;

    void Start()
    {
        AudioManager.PlayBGM(titleBGM);  // タイトルBGMを再生
        // 選択ボタンの設定
        lastSelectedObject = UIUtils.SetFocusWithoutSound(titleFirstSelectionButton);
    }

    void Update()
    {
        // キーボードやゲームパッド操作によるボタン移動を監視し移動音を鳴らす
        UIUtils.HandleselectionChangeSE(ref lastSelectedObject);
    }

    #region スタートボタン, ウィンドウ表示・非表示処理 ----

    /// <summary>
    /// スタートボタン押下時の処理
    /// </summary>
    public void OnStartButtonPress()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        SceneManager.LoadScene("02_GameScene");  // ゲームシーンに移動
    }

    /// <summary>
    /// ランキングウィンドウを開く処理
    /// </summary>
    public void OpenRankingWindow()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        SetTitleButtonsInteractable(false);  // タイトルメインボタン群の操作を無効化
        rankingWindow.SetActive(true);  // ランキングウィンドウを表示させる

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
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        SetTitleButtonsInteractable(true);  // タイトルメインボタン群の操作を有効化
        rankingWindow.SetActive(false);  // ランキングウィンドウを非表示にする

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
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        SetTitleButtonsInteractable(false);  // タイトルメインボタン群の操作を無効化
        systemWindow.SetActive(true);  // システムウィンドウを表示させる

        // マスター音量オブジェクトを選択状態にする
        if (systemMasterSliderObject != null)
        {
            lastSelectedObject = UIUtils.SetFocusWithoutSound(systemMasterSliderObject);
        }
    }

    /// <summary>
    /// システムウィンドウを閉じる処理
    /// </summary>
    public void CloseSystemWindow()
    {
        AudioManager.PlaySESubmit();  // 決定音を鳴らす
        SetTitleButtonsInteractable(true);  // タイトルメインボタン群の操作を有効化
        systemWindow.SetActive(false); // システムウィンドウを非表示にする

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
        // ガード節
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
