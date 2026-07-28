// ResultManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
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
