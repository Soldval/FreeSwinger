// ResultManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// リザルトシーン全体を管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    void Start()
    {
        // BGMをストップさせる
        if(AudioManager.instance != null)
        {
            AudioManager.instance.StopBGM();
        }
    }

    // もう一回！ボタン処理
    public void OnOneMoreTimeButtonClick()
    {
        SceneManager.LoadScene("02_GameScene");
    }

    // タイトル移動ボタン処理
    public void ReturnToTitleButtonClick()
    {
        SceneManager.LoadScene("01_TitleScene");
    }

}
