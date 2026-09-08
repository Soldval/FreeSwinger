// SoundVolumeManager.cs

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// サウンド調整を管理するクラス
/// </summary>
public class SoundVolumeManager : MonoBehaviour
{
    [Header("AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("UI_Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    // AudioMixerの「Exposed Parameter」名をセット
    private const string MASTER_PARAM = "MasterVolume";
    private const string BGM_PARAM = "BGMVolume";
    private const string SE_PARAM = "SEVolume";

    void Start()
    {
        // 各スライダー操作時に音量変更するイベントを登録
        if(masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if(bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (seSlider != null)
        {
            seSlider.onValueChanged.AddListener(SetSEVolume);
        }
    }

    /// <summary>
    /// マスター音量を変更する処理
    /// </summary>
    /// <param name="sliderValue">スライダー値</param>
    public void SetMasterVolume(float sliderValue)
    {
        AudioManager.PlaySESlider();  // スライダー操作音を鳴らす
        SetVolumeInternal(MASTER_PARAM, sliderValue);  // スライダー値からマスター音量を変更する
    }

    /// <summary>
    /// BGM音量を変更する処理
    /// </summary>
    /// <param name="sliderValue">スライダー値</param>
    public void SetBGMVolume(float sliderValue)
    {
        AudioManager.PlaySESlider();  // スライダー操作音を鳴らす
        SetVolumeInternal(BGM_PARAM, sliderValue);  // スライダー値からBGM音量を変更する
    }

    /// <summary>
    /// SE音量を変更する処理
    /// </summary>
    /// <param name="sliderValue">スライダー値</param>
    public void SetSEVolume(float sliderValue)
    {
        AudioManager.PlaySESlider();  // スライダー操作音を鳴らす
        SetVolumeInternal(SE_PARAM, sliderValue);  // スライダー値からSE音量を変更する
    }

    /// <summary>
    /// スライダー値をdB変換してセットする処理
    /// </summary>
    /// <param name="paramName">変換するパラメータ名</param>
    /// <param name="sliderValue">変換するスライダー値</param>
    private void SetVolumeInternal(string paramName, float sliderValue)
    {
        if(sliderValue <= 0)
        {
            // 0の時はミュートにセット
            audioMixer.SetFloat(paramName, -80f);
        }
        else
        {
            // 1～10を-36dB～0dBに変換してセット
            float dB = (sliderValue - 10f) * 4f;
            audioMixer.SetFloat(paramName, dB);
        }
    }
}
