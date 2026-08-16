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

    /// <summary>
    /// ボタン決定音を鳴らす
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
    /// ボタン移動音を鳴らす
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
    /// 指定したBGMでループ再生する
    /// </summary>
    /// <param name="bgm">再生したいBGMデータ</param>
    public static void PlayBGM(AudioClip bgm)
    {
        // AudioManagerのinstance, 再生したいBGMデータ, BGM用スピーカーいずれかが空の場合を回避
        if (instance == null || bgm == null || instance.bgmAudioSource == null) return;

        // すでに同じ曲が流れている場合、そのまま流す(無駄な途切れ防止)
        if (instance.bgmAudioSource.clip == bgm && instance.bgmAudioSource.isPlaying) return;

        instance.bgmAudioSource.clip = bgm;  // bgmをセット
        instance.bgmAudioSource.time = 0f;  // bgmを始めから流す
        instance.bgmAudioSource.loop = true;  // ループON
        instance.bgmAudioSource.Play();
    }

    /// <summary>
    /// BGMを停止させる
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

    /// <summary>
    /// 効果音再生の共通処理(ワンショット)
    /// </summary>
    /// <param name="clip">再生する効果音データ</param>
    private void PlaySE(AudioClip clip)
    {
        // 効果音データが空ではないかつスピーカーがセットされている場合
        if (clip != null && seAudioSource != null)
        {
            // ワンショットで効果音を鳴らす
            seAudioSource.PlayOneShot(clip);
        }
    }
}
