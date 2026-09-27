// UIUtils.cs

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI操作に関連する共通処理をまとめたクラス
/// </summary>
public static class UIUtils
{
    /// <summary>
    /// オブジェクトを選択状態にする(効果音は鳴らさない)
    /// </summary>
    /// <param name="target">選択状態にしたいオブジェクト</param>
    /// <returns>選択状態にしたいオブジェクト GameObject</returns>
    public static GameObject SetFocusWithoutSound(GameObject target)
    {
        if (target == null || EventSystem.current == null) return null;

        EventSystem.current.SetSelectedGameObject(null);  // 現在の選択オブジェクトを空にする
        EventSystem.current.SetSelectedGameObject(target);  // 選択したいオブジェクトを選択状態にする

        // 前回選択オブジェクト更新用にそのまま選択状態にしたいオブジェクトを返す
        // (二重に効果音が鳴らないための防止)
        return target;
    }

    /// <summary>
    /// ボタン選択切り替え時にSEを鳴らす処理
    /// </summary>
    /// <param name="lastSelected">前回選択オブジェクトの記憶</param>
    public static void HandleselectionChangeSE(ref GameObject lastSelected)
    {
        if (EventSystem.current == null) return;
        
        // 現在選択状態になっているボタンを取得する
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // 現在選択中が無い、もしくは前回選択と同じ場合は処理を終了。
        if(currentSelected == null || currentSelected == lastSelected) return;
        
        // 初期フォーカス以外は移動音を鳴らす
        if (lastSelected != null && AudioManager.instance != null)
        {
            AudioManager.PlaySEMove();
        }

        // 現在の選択ボタンを記憶させる
        lastSelected = currentSelected;
    }
}
