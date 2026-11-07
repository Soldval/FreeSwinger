// LoadingManager.cs

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

/// <summary>
/// ロードシーン全体を管理するクラス
/// </summary>
public class LoadingManager : MonoBehaviour
{
    [Header("参照するUI一覧")]
    [SerializeField] private TextMeshProUGUI txtLoading;  // 「ロード中」テキスト
    [SerializeField] private RectTransform loadingIcon;  // 円形アイコン
    
    [Header("設定一覧")]
    [SerializeField] private float minLoadingTime = 1.5f;  // 最低表示時間
    [SerializeField] private float rotateSpeed = 1.0f;  // アイコンが1回転する時間

    // 内部変数
    private Tween fadeTween;
    private Tween rotateTween;

    void Start()
    {
        // 「ロード中」テキストの点滅アニメーション
        if(txtLoading != null)
        {
            fadeTween = txtLoading.DOFade(0.2f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // 円形アイコンの回転アニメーション
        if(loadingIcon != null)
        {
            rotateTween = loadingIcon.DORotate(new Vector3(0, 0, -360f), rotateSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        StartCoroutine(LoadNextSceneCoroutine());
    }

    /// <summary>
    /// 非同期ロードで次のシーンへ移動する処理(コルーチン)
    /// </summary>
    /// <returns>待機時間 IEnumerator</returns>
    private IEnumerator LoadNextSceneCoroutine()
    {
        string targetScene = SceneBridge.NextSceneName;

        if(string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("移動先シーン名が設定されていません");
            yield break;
        }

        // 非同期読み込みを開始
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);

        // 読み込み完了後に自動でシーン切替を実行しない(安全制御用)
        asyncLoad.allowSceneActivation = false;

        float timer = 0f;

        while(!asyncLoad.isDone)
        {
            timer += Time.deltaTime;

            // ロードが完了し最低表示時間も経過した場合、シーン切替を許可する。
            if(asyncLoad.progress >= 0.9f && timer >= minLoadingTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    /// <summary>
    /// LoadingManager破棄時にTweenを破棄する処理
    /// </summary>
    private void OnDestroy()
    {
        fadeTween?.Kill();
        rotateTween?.Kill();
    }
}
