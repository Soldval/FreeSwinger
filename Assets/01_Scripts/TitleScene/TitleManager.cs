// TitleManager.cs

using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // ウィンドウ(フォルダ)を登録
    [SerializeField] private GameObject RankingWindow;  // ランキングウィンドウ
    [SerializeField] private GameObject SystemWindow;  // システムウィンドウ

    // スタートボタン処理
    public void OnStartButtonClick()
    {
        SceneManager.LoadScene("02_GameScene");
        //Debug.Log("スタートボタンを押しました！");
    }

    // ランキングボタン処理
    public void OpenRankingWindow()
    {
        RankingWindow.SetActive(true);
        //Debug.Log("ランキングウィンドウを開きました！");
    }

    public void CloseRankingWindow()
    {
        RankingWindow.SetActive(false);
        //Debug.Log("ランキングウィンドウを閉じました！");
    }

    // システムボタン処理
    public void OpenSystemWindow()
    {
        SystemWindow.SetActive(true);
        //Debug.Log("システムウィンドウを開きました！");
    }

    public void CloseSystemWindow()
    {
        SystemWindow.SetActive(false);
        //Debug.Log("システムウィンドウを閉じました！");
    }

}
