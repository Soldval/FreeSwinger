// SoundVolumeManager.cs

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UIElements;

using Slider = UnityEngine.UI.Slider;
using Image = UnityEngine.UI.Image;

/// <summary>
/// サウンド調整・画面明暗度のシステム設定を管理するクラス
/// </summary>
public class SystemOptionManager : MonoBehaviour
{
    // 共通使用のためのインスタンス(シングルトン)
    public static SystemOptionManager instance {  get; private set; }

    [Header("AudioMixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("UI_Sliders(Sound)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;

    [Header("UI_Sliders(Brightness)")]
    [SerializeField] private Slider brightnessSlider;

    [Header("Brightness Overlay")]
    [SerializeField] private Image brightnessOverlayImage;

    // AudioMixerの「Exposed Parameter」名をセット
    private const string MASTER_PARAM = "MasterVolume";
    private const string BGM_PARAM = "BGMVolume";
    private const string SE_PARAM = "SEVolume";

    // PlayerPrefs保存用のキー名
    private const string MASTER_KEY = "Save_MasterVolume";
    private const string BGM_KEY = "Save_BGMVolume";
    private const string SE_KEY = "Save_SEVolume";
    private const string BRIGHTNESS_KEY = "Save_Brightness";

    // デフォルト設定値
    private const float DEFAULT_VOLUME = 8.0f;
    private const float DEFAULT_BRIGHTNESS = 8.0f;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if(instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        //LoadAndApplySettings();
        SetupSlider();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindBrightnessOverlayInCurrentScene();

        float brightnessVal = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, DEFAULT_VOLUME);
        SetBrightnessInternal(brightnessVal);

        RestoreSliderValues();
    }

    private void RestoreSliderValues()
    {
        // シーン内の全てのスライダー(非アクティブも含む)を検索し名前で割り当てる
        Slider[] allSliders = Resources.FindObjectsOfTypeAll<Slider>();
        foreach(var slider in allSliders)
        {
            // シーン上に存在するものだけ対象にする
            if (!slider.gameObject.scene.IsValid()) continue;

            if (slider.name == "SW_Slider_Master")
            {
                masterSlider = slider;
            }
            else if (slider.name == "SW_Slider_BGM")
            {
                bgmSlider = slider;
            }
            else if (slider.name == "SW_Slider_SE")
            {
                seSlider = slider;
            }
            else if (slider.name == "SW_Slider_Brightness")
            {
                brightnessSlider = slider;
            }
        }

        // 保存されている設定値を読み込む
        float masterVol = PlayerPrefs.GetFloat(MASTER_KEY, DEFAULT_VOLUME);
        float bgmVol = PlayerPrefs.GetFloat(BGM_KEY, DEFAULT_VOLUME);
        float seVol = PlayerPrefs.GetFloat(SE_KEY, DEFAULT_VOLUME);
        float brightnessVal = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, DEFAULT_BRIGHTNESS);

        // 値を代入する前にリスナーをクリアし誤作動を防ぐ
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveAllListeners(); // 多重登録を防ぐため一旦クリア
            masterSlider.value = masterVol;
            masterSlider.onValueChanged.AddListener((val) => {
                SetMasterVolume(val);
                PlayerPrefs.SetFloat(MASTER_KEY, val);
            });
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.value = bgmVol;
            bgmSlider.onValueChanged.AddListener((val) => {
                SetBGMVolume(val);
                PlayerPrefs.SetFloat(BGM_KEY, val);
            });
        }

        if (seSlider != null)
        {
            seSlider.onValueChanged.RemoveAllListeners();
            seSlider.value = seVol;
            seSlider.onValueChanged.AddListener((val) => {
                SetSEVolume(val);
                PlayerPrefs.SetFloat(SE_KEY, val);
            });
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveAllListeners();
            brightnessSlider.value = brightnessVal;
            brightnessSlider.onValueChanged.AddListener((val) => {
                SetBrightness(val);
                PlayerPrefs.SetFloat(BRIGHTNESS_KEY, val);
            });
        }
    }

    private void FindBrightnessOverlayInCurrentScene()
    {
        GameObject blackObj = GameObject.Find("BlackImage");
        if(blackObj != null)
        {
            brightnessOverlayImage = blackObj.GetComponent<Image>();
        }
        else
        {
            brightnessOverlayImage= null;
        }
    }

    /// <summary>
    /// 各スライダーの初期設定とイベント登録を行う処理
    /// </summary>
    private void SetupSlider()
    {
        // 保存されている各音量設定を読み込む
        float masterVol = PlayerPrefs.GetFloat(MASTER_KEY, DEFAULT_VOLUME);
        float bgmVol = PlayerPrefs.GetFloat(BGM_KEY, DEFAULT_VOLUME);
        float seVol = PlayerPrefs.GetFloat(SE_KEY, DEFAULT_VOLUME);
        float brightnessVal = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, DEFAULT_BRIGHTNESS);

        // 各音量・明暗度設定を反映
        SetVolumeInternal(MASTER_PARAM, masterVol);
        SetVolumeInternal(BGM_PARAM, bgmVol);
        SetVolumeInternal(SE_PARAM, seVol);
        SetBrightnessInternal(brightnessVal);

        // 各UIスライダーに初期値をセット
        if (masterSlider != null)
        {
            masterSlider.value = masterVol;
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVol;
        }

        if (seSlider != null)
        {
            seSlider.value = seVol;
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.value = brightnessVal;
        }

        // 各スライダー操作時に音量変更するイベントを登録
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.AddListener((val) =>
            {
                SetMasterVolume(val);
                PlayerPrefs.SetFloat(MASTER_KEY, val);
            });
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener((val) =>
            {
                SetBGMVolume(val);
                PlayerPrefs.SetFloat(BGM_KEY, val);
            });
        }

        if (seSlider != null)
        {
            seSlider.onValueChanged.AddListener((val) =>
            {
                SetSEVolume(val);
                PlayerPrefs.SetFloat(SE_KEY, val);
            });
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.AddListener((val) =>
            {
                SetBrightness(val);
                PlayerPrefs.SetFloat(BRIGHTNESS_KEY, val);
            });
        }
    }

    private void LoadAndApplySettings()
    {
        // 保存されている各音量設定を読み込む
        float masterVol = PlayerPrefs.GetFloat(MASTER_KEY, DEFAULT_VOLUME);
        float bgmVol = PlayerPrefs.GetFloat(BGM_KEY, DEFAULT_VOLUME);
        float seVol = PlayerPrefs.GetFloat(SE_KEY, DEFAULT_VOLUME);
        float brightnessVal = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, DEFAULT_BRIGHTNESS);

        // 各音量・明暗度設定を反映
        SetVolumeInternal(MASTER_PARAM, masterVol);
        SetVolumeInternal(BGM_PARAM, bgmVol);
        SetVolumeInternal(SE_PARAM, seVol);
        SetBrightnessInternal(brightnessVal);
    }

    #region 音量調整処理まとめ ----

    /// <summary>
    /// マスター音量を変更する処理
    /// </summary>
    /// <param name="sliderValue">スライダー値</param>
    public void SetMasterVolume(float sliderValue)
    {
        AudioManager.PlaySESlider();
        SetVolumeInternal(MASTER_PARAM, sliderValue);
    }

    /// <summary>
    /// BGM音量を変更する処理
    /// </summary>
    /// <param name="sliderValue">スライダー値</param>
    public void SetBGMVolume(float sliderValue)
    {
        AudioManager.PlaySESlider();
        SetVolumeInternal(BGM_PARAM, sliderValue);
    }

    /// <summary>
    /// SE音量を変更する処理
    /// </summary>
    /// <param name="sliderValue">スライダー値</param>
    public void SetSEVolume(float sliderValue)
    {
        AudioManager.PlaySESlider();
        SetVolumeInternal(SE_PARAM, sliderValue);
    }

    /// <summary>
    /// スライダー値をdB変換してセットする処理
    /// </summary>
    /// <param name="paramName">変換するパラメータ名</param>
    /// <param name="sliderValue">変換するスライダー値</param>
    private void SetVolumeInternal(string paramName, float sliderValue)
    {
        if (audioMixer == null) return;

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

    #endregion // 音量調整処理まとめ ----

    #region 明暗度調整処理まとめ

    /// <summary>
    /// 明暗度を変更する処理
    /// </summary>
    /// <param name="sliderValue">スライダー値</param>
    public void SetBrightness(float sliderValue)
    {
        AudioManager.PlaySESlider();
        SetBrightnessInternal(sliderValue);
        PlayerPrefs.SetFloat(BRIGHTNESS_KEY, sliderValue);
        PlayerPrefs.Save();
    }

    public void SetBrightnessInternal(float sliderValue)
    {
        if (brightnessOverlayImage == null) return;

        // 1～10を0.0～0.6の不透明度に変換する
        float maxAlpha = 0.6f;
        float normalizedVal = (sliderValue - 1f) / 9f;
        float targetAlpha = (1f - normalizedVal) * maxAlpha;

        Color color = brightnessOverlayImage.color;
        color.a = targetAlpha;
        brightnessOverlayImage.color = color;
    }

    #endregion // 明暗度調整処理まとめ ----

}
