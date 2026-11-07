// UIUtils.cs

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI操作に関連する共通クラス
/// </summary>
public static class UIUtils
{
    /// <summary>
    /// オブジェクトをフォーカスにする(SEは鳴らさない)
    /// </summary>
    /// <param name="target">フォーカスにするオブジェクト</param>
    /// <returns>フォーカスにするオブジェクト GameObject</returns>
    public static GameObject SetFocusWithoutSound(GameObject target)
    {
        if (target == null || EventSystem.current == null) return null;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);

        return target;
    }

    /// <summary>
    /// ボタン選択切替時にSEを鳴らす処理
    /// </summary>
    /// <param name="lastSelected">前回選択オブジェクトの記憶</param>
    public static void HandleselectionChangeSE(ref GameObject lastSelected)
    {
        if (EventSystem.current == null) return;
        
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // 現在選択中が無いもしくは前回選択と同じ場合は処理を終了する
        if(currentSelected == null || currentSelected == lastSelected) return;
        
        // 初期フォーカス以外は移動音を鳴らす
        if (lastSelected != null && AudioManager.instance != null)
        {
            AudioManager.PlaySEMove();
        }

        lastSelected = currentSelected;
    }
}
