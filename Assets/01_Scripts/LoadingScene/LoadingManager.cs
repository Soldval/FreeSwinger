// LoadingManager.cs

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using System;

/// <summary>
/// ロードシーン全体を管理するクラス
/// </summary>
public class LoadingManager : MonoBehaviour
{
    [Header("参照するUI一覧")]
    [SerializeField] private Slider progressBar;  // 進捗バー
    [SerializeField] private TextMeshProUGUI txtPercent;  // 「〇〇%」テキスト
    [SerializeField] private RectTransform loadingIcon;  // 円形アイコン
    [SerializeField] private TextMeshProUGUI txtLoading;  // 「ロード中」テキスト

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

        // 非同期ロード開始
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

        // 読み込み完了後に自動でシーン切り替えを行わせない(安全制御用)
        asyncLoad.allowSceneActivation = false;

        float timer = 0f;

        while(!asyncLoad.isDone)
        {
            timer += Time.deltaTime;

            // asyncLoad.progressは 0.0～0.9 の値をとる(0.9でロード完了)
            float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // 最低表示時間を考慮した進捗率の計算
            float timeProgress = Mathf.Clamp01(timer / minLoadingTime);
            float currentProgress = Mathf.Min(targetProgress, timeProgress);

            // UIの更新
            //if (progressBar != null) progressBar.value = currentProgress;
            //if(txtPercent != null) txtPercent.text = (currentProgress * 100f).ToString("F0") + "%";

            // ロードが完了し、最低表示時間も経過した場合。
            if(asyncLoad.progress >= 0.9f && timer >= minLoadingTime)
            {
                // シーン切り替えを許可
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    /// <summary>
    /// このオブジェクト破棄時にTweenを安全停止させる処理
    /// </summary>
    private void OnDestroy()
    {
        fadeTween?.Kill();
        rotateTween?.Kill();
    }
}
