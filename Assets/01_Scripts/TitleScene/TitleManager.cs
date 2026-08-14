// TitleManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
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
    [SerializeField] private Button rankingCloseButton;     // ランキングウィンドウの閉じるボタン
    [SerializeField] private GameObject rankingOpenButton;  // ランキングウィンドウを開くボタン

    [Header("システムウィンドウ関連")]
    [SerializeField] private GameObject systemWindow;      // システムウィンドウ
    [SerializeField] private Button systemCloseButton;     // システムウィンドウの閉じるボタン
    [SerializeField] private GameObject systemOpenButton;  // システムウィンドウを開くボタン

    // 前回選択オブジェクトの記憶
    private GameObject lastSelectedObject;

    void Start()
    {
        // タイトルBGMの再生
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(titleBGM);
        }

        // 選択ボタンの設定(効果音は鳴らさない)
        SetFocusWithoutSound(titleFirstSelectionButton);
    }

    void Update()
    {
        // ------------------------------------------------------------
        // ---- キーボードやゲームパッド操作によるボタン移動を監視 ----
        // ------------------------------------------------------------
        if(EventSystem.current != null)
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            
            // 選択中ボタンがあり、それが前回選択ボタンと異なっている場合。
            if(currentSelected != null && currentSelected != lastSelectedObject)
            {
                // 初期化時以外は移動音を鳴らす
                if(lastSelectedObject != null && AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySEMove();
                }

                // 現在の選択ボタンを記憶させる
                lastSelectedObject = currentSelected;
            }
        }
    }

    #region スタートボタン, ウィンドウ表示・非表示処理 ----

    // スタートボタン処理
    public void OnStartButtonPress()
    {
        if(AudioManager.instance != null)
        {
            AudioManager.instance.PlaySESubmit();  // 決定音を鳴らす
        }

        SceneManager.LoadScene("02_GameScene");
        //Debug.Log("スタートボタンを押しました！");
    }

    // ランキングウィンドウ表示処理
    public void OpenRankingWindow()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySESubmit();  // 決定音を鳴らす
        }

        SetTitleButtonsInteractable(false);  // タイトルメインボタン群の操作を無効化
        rankingWindow.SetActive(true);

        if(rankingCloseButton != null)
        {
            SetFocusWithoutSound(rankingCloseButton.gameObject);
        }
        //Debug.Log("ランキングウィンドウを開きました！");
    }

    // ランキングウィンドウ非表示処理
    public void CloseRankingWindow()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySESubmit();  // 決定音を鳴らす
        }

        SetTitleButtonsInteractable(true);  // タイトルメインボタン群の操作を有効化
        rankingWindow.SetActive(false);

        SetFocusWithoutSound(rankingOpenButton);
        //Debug.Log("ランキングウィンドウを閉じました！");
    }

    // システムウィンドウ表示処理
    public void OpenSystemWindow()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySESubmit();  // 決定音を鳴らす
        }

        SetTitleButtonsInteractable(false);  // タイトルメインボタン群の操作を無効化
        systemWindow.SetActive(true);

        if (systemCloseButton != null)
        {
            SetFocusWithoutSound(systemCloseButton.gameObject);
        }
        //Debug.Log("システムウィンドウを開きました！");
    }

    public void CloseSystemWindow()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySESubmit();  // 決定音を鳴らす
        }

        SetTitleButtonsInteractable(true);  // タイトルメインボタン群の操作を有効化
        systemWindow.SetActive(false);

        SetFocusWithoutSound(systemOpenButton);
        //Debug.Log("システムウィンドウを閉じました！");
    }

    #endregion // スタートボタン, ウィンドウ表示・非表示処理 ----

    /// <summary>
    /// タイトルのメインボタン群の操作可能状態(有効/無効)を一括切り替えする
    /// </summary>
    /// <param name="interactable">true:操作可能 / false:操作不可能</param>
    private void SetTitleButtonsInteractable(bool interactable)
    {
        // ボタンリストが空の場合、ここで処理をやめる。
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
    
    /// <summary>
    /// 選択したオブジェクトを選択状態にする(効果音は鳴らさない)
    /// </summary>
    /// <param name="target">選択状態にしたいオブジェクト(ボタン)</param>
    private void SetFocusWithoutSound(GameObject target)
    {
        if(target != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);  // 現在の選択オブジェクトを空にする
            EventSystem.current.SetSelectedGameObject(target);  // 選択したいオブジェクトを選択状態にする

            // 選択ボタン記憶を更新させる(Update側で二重に効果音が鳴らないための防止)
            lastSelectedObject = target;
        }
    }
}
