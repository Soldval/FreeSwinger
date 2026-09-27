// UIOutlinePulseOnSelectDOTween.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// フォーカス状態のUIのOutlineをDOTweenで点滅させるクラス
/// </summary>
[RequireComponent(typeof(Outline))]
public class UIOutlinePulseOnSelectDOTween : MonoBehaviour
{
    [Header("フォーカスを監視する対象のスライダー(未割り当て時は自動取得)")]
    [SerializeField] private Selectable targetSelectable;

    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float minAlpha = 0.1f;

    // 内部変数
    private Outline outline;
    private Tween pulseTween;
    private bool isPulsing = false;

    void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline != null)
        {
            outline.enabled = false;
        }

        // 監視対象オブジェクトが空の場合、子オブジェクトを自動取得する。
        if(targetSelectable == null)
        {
            targetSelectable = GetComponentInChildren<Selectable>();
        }
    }

    void Update()
    {
        if (targetSelectable == null || EventSystem.current == null) return;

        bool isFocused = (EventSystem.current.currentSelectedGameObject == targetSelectable.gameObject);

        if(isFocused && !isPulsing)
        {
            StartPulse();
        }
        else if(!isFocused && isPulsing)
        {
            StopPulse();
        }
    }

    /// <summary>
    /// outlineの点滅アニメーションを開始する処理
    /// </summary>
    private void StartPulse()
    {
        if (outline == null) return;

        isPulsing = true;
        outline.enabled = true;

        pulseTween?.Kill();

        Color color = outline.effectColor;
        color.a = 1.0f;
        outline.effectColor = color;

        pulseTween = outline.DOFade(minAlpha, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    /// <summary>
    /// outlineの点滅アニメーションを停止し非表示にする処理
    /// </summary>
    private void StopPulse()
    {
        isPulsing = false;
        pulseTween?.Kill();
        if (outline != null) outline.enabled = false;
    }

    /// <summary>
    /// 非アクティブ時にアニメーションを破棄・安全停止させる処理
    /// </summary>
    void OnDisable()
    {
        StopPulse();
    }
}
