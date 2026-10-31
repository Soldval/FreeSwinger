// AudioManager.cs

using UnityEngine;

/// <summary>
/// BGMやSEの再生を管理する共通クラス
/// </summary>
public class AudioManager : MonoBehaviour
{
    // インスタンス(シングルトン)
    public static AudioManager instance;

    [Header("再生用スピーカー")]
    [SerializeField] private AudioSource seAudioSource;  // SE用スピーカー
    [SerializeField] private AudioSource bgmAudioSource;  // BGM用スピーカー

    [Header("UI効果音素材(共通)")]
    
    [SerializeField] private AudioClip seButtonMove;  // ボタン移動音
    [SerializeField] private AudioClip seButtonSubmit;  // ボタン決定音
    [SerializeField] private AudioClip seSliderMove; // スライダー操作音

    [Header("UI効果音素材(タイトルシーン)")]
    [SerializeField] private AudioClip sePressAnyButton;  // PRESS ANY BUTTON 効果音

    [Header("UI効果音素材(ゲームシーン)")]
    [SerializeField] private AudioClip seCountDown;  // ゲーム開始前カウントダウン効果音
    [SerializeField] private AudioClip seGameStart;  // ゲーム開始効果音
    [SerializeField] private AudioClip seTimeUp;  // タイムアップ効果音
    [SerializeField] private AudioClip seLeftSwing;  // 左スイング効果音

    [Header("UI効果音素材(リザルトシーン)")]
    [SerializeField] private AudioClip seCountUp;  // カウントアップ効果音
    [SerializeField] private AudioClip seHighGrade;  // 高成績効果音
    [SerializeField] private AudioClip seMiddleGrade;  // 中成績効果音
    [SerializeField] private AudioClip seLowGrade;  // 低成績効果音
    [SerializeField] private AudioClip seCheer;  // 歓声効果音
    [SerializeField] private AudioClip seApplause;  // 拍手効果音

    private void Awake()
    {
        // AudioManagerのインスタンスはシングルトン
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region 共通SE処理まとめ ----

    /// <summary>
    /// ボタン決定音を鳴らす処理
    /// </summary>
    public static void PlaySESubmit()
    {
        if (instance == null) return;
        
        instance.PlaySE(instance.seButtonSubmit);
    }

    /// <summary>
    /// ボタン移動音を鳴らす処理
    /// </summary>
    public static void PlaySEMove()
    {
        if (instance == null) return;
        
        instance.PlaySE(instance.seButtonMove);
    }

    /// <summary>
    /// スライダー操作音を鳴らす処理
    /// </summary>
    public static void PlaySESlider()
    {
        if (instance == null) return;
        
        instance.PlaySE(instance.seSliderMove);
    }

    #endregion // 共通SE処理まとめ

    #region タイトルシーンSE処理まとめ ----

    /// <summary>
    /// PRESS ANY BUTTON 効果音を鳴らす処理
    /// </summary>
    public static void PlaySEPressAnyButton()
    {
        if (instance == null) return;

        instance.PlaySE(instance.sePressAnyButton);
    }

    #endregion // タイトルシーンSE処理まとめ ----

    #region ゲームシーンSE処理まとめ ----

    /// <summary>
    /// スタート前カウントダウン効果音を鳴らす処理
    /// </summary>
    public static void PlaySECountDown()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seCountDown);
    }

    /// <summary>
    /// ゲームスタート効果音を鳴らす処理
    /// </summary>
    public static void PlaySEGameStart()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seGameStart);
    }

    /// <summary>
    /// タイムアップ効果音を鳴らす処理
    /// </summary>
    public static void PlaySETimeUp()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seTimeUp);
    }

    /// <summary>
    /// 左スイング効果音を鳴らす処理
    /// </summary>
    public static void PlaySELeftSwing()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seLeftSwing);
    }

    #endregion // ゲームシーンSE処理まとめ ----

    #region リザルトシーンSE処理まとめ ----

    /// <summary>
    /// カウントアップ効果音を鳴らす処理
    /// </summary>
    public static void PlaySECountUp()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seCountUp);
    }

    /// <summary>
    /// 高成績効果音を鳴らす処理
    /// </summary>
    public static void PlaySEHighGrade()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seHighGrade);
    }

    /// <summary>
    /// 中成績効果音を鳴らす処理
    /// </summary>
    public static void PlaySEMiddleGrade()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seMiddleGrade);
    }

    /// <summary>
    /// 低成績効果音を鳴らす処理
    /// </summary>
    public static void PlaySELowGrade()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seLowGrade);
    }

    /// <summary>
    /// 歓声効果音を鳴らす処理
    /// </summary>
    public static void PlaySECheer()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seCheer);
    }

    /// <summary>
    /// 拍手効果音を鳴らす処理
    /// </summary>
    public static void PlaySEApplause()
    {
        if (instance == null) return;

        instance.PlaySE(instance.seApplause);
    }

    #endregion // リザルトシーンSE処理まとめ ----

    #region その他SE処理まとめ ----

    /// <summary>
    /// 効果音再生の共通処理(ワンショット)
    /// </summary>
    /// <param name="clip">再生する効果音データ</param>
    private void PlaySE(AudioClip clip)
    {
        if (clip == null || seAudioSource == null) return;
        
        seAudioSource.PlayOneShot(clip);
    }

    #endregion // その他SE処理まとめ ----

    #region BGM処理まとめ ----

    /// <summary>
    /// 指定したBGMでループ再生する処理
    /// </summary>
    /// <param name="bgm">再生したいBGMデータ</param>
    public static void PlayBGM(AudioClip bgm)
    {
        if (instance == null || bgm == null || instance.bgmAudioSource == null) return;

        instance.bgmAudioSource.clip = bgm;
        instance.bgmAudioSource.time = 0f;  // BGMを始めから流す
        instance.bgmAudioSource.loop = true;
        instance.bgmAudioSource.Play();
    }

    /// <summary>
    /// BGM停止の処理
    /// </summary>
    public static void StopBGM()
    {
        if (instance == null || instance.bgmAudioSource == null) return;
        
        instance.bgmAudioSource.Stop();
        instance.bgmAudioSource.clip = null;  // 割当BGMデータをクリア
    }

    #endregion // BGM処理まとめ
}
