// AudioManager.cs

using UnityEngine;

/// <summary>
/// BGMやSEの再生を管理するクラス
/// </summary>
public class AudioManager : MonoBehaviour
{
    // どこからでも呼び出せるようにする(シングルトン)
    public static AudioManager instance;

    [Header("再生用スピーカー")]
    [SerializeField] private AudioSource seAudioSource;  // SE用スピーカー
    [SerializeField] private AudioSource bgmAudioSource;  // BGM用スピーカー

    [Header("UI効果音素材")]
    [SerializeField] private AudioClip seButtonMove;  // ボタン移動音
    [SerializeField] private AudioClip seButtonSubmit;  // ボタン決定音
    [SerializeField] private AudioClip seSliderMove; // スライダー操作音

    private void Awake()
    {
        // AudioManagerのinstanceはシーン内に1つのみにする(シングルトンパターン)
        if(instance == null)
        {
            instance = this;  // 自分自身のinstanceを登録
            DontDestroyOnLoad(gameObject);  // シーンが切り替わってもinstanceを保護する
        }
        else
        {
            // AudioManagerのinstanceが重複したら2個目はすぐに削除
            Destroy(gameObject);
        }
    }

    #region SE処理まとめ ----

    /// <summary>
    /// ボタン決定音を鳴らす処理
    /// </summary>
    public static void PlaySESubmit()
    {
        // AudioManagerのinstanceがある場合
        if(instance != null)
        {
            instance.PlaySE(instance.seButtonSubmit);
        }
    }

    /// <summary>
    /// ボタン移動音を鳴らす処理
    /// </summary>
    public static void PlaySEMove()
    {
        // AudioManagerのinstanceがある場合
        if (instance != null)
        {
            instance.PlaySE(instance.seButtonMove);
        }
    }

    /// <summary>
    /// スライダー操作音を鳴らす処理
    /// </summary>
    public static void PlaySESlider()
    {
        // AudioManagerのinstanceがある場合
        if (instance != null)
        {
            instance.PlaySE(instance.seSliderMove);
        }
    }

    /// <summary>
    /// 効果音再生の共通処理(ワンショット)
    /// </summary>
    /// <param name="clip">再生する効果音データ</param>
    private void PlaySE(AudioClip clip)
    {
        if (clip != null && seAudioSource != null)
        {
            seAudioSource.PlayOneShot(clip);
        }
    }

    #endregion // SE処理まとめ

    #region BGM処理まとめ ----

    /// <summary>
    /// 指定したBGMでループ再生する処理
    /// </summary>
    /// <param name="bgm">再生したいBGMデータ</param>
    public static void PlayBGM(AudioClip bgm)
    {
        // AudioManagerのinstance, 再生したいBGMデータ, BGM用スピーカーいずれかが空の場合を回避
        if (instance == null || bgm == null || instance.bgmAudioSource == null) return;

        // すでに同じ曲が流れている場合、そのまま流す(無駄な途切れ防止)
        if (instance.bgmAudioSource.clip == bgm && instance.bgmAudioSource.isPlaying) return;

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
        // AudioManagerのinstanceがありBGM用スピーカーがある場合
        if (instance != null && instance.bgmAudioSource != null)
        {
            instance.bgmAudioSource.Stop();
            instance.bgmAudioSource.clip = null;  // 割当BGMデータをクリア
        }
    }

    #endregion // BGM処理まとめ
}
