// UIUtils.cs

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI操作に関連する共通処理をまとめたクラス
/// </summary>
public static class UIUtils
{
    /// <summary>
    /// 選択したオブジェクトを選択状態にする(効果音は鳴らさない)
    /// </summary>
    /// <param name="target">選択状態にしたいオブジェクト(ボタン)</param>
    /// <returns>選択状態にしたいオブジェクト(ボタン)</returns>
    public static GameObject SetFocusWithoutSound(GameObject target)
    {
        if (target != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);  // 現在の選択オブジェクトを空にする
            EventSystem.current.SetSelectedGameObject(target);  // 選択したいオブジェクトを選択状態にする
        }

        // 前回選択オブジェクト更新用にそのまま選択状態にしたいオブジェクト(ボタン)を返す
        // (二重に効果音が鳴らないための防止)
        return target;
    }

    public static void HandleselectionChangeSE(ref GameObject lastSelected)
    {
        if (EventSystem.current != null)
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            // 選択中ボタンがあり、それが前回選択ボタンと異なっている場合。
            if (currentSelected != null && currentSelected != lastSelected)
            {
                // 初期化時以外は移動音を鳴らす
                if (lastSelected != null && AudioManager.instance != null)
                {
                    AudioManager.PlaySEMove();
                }

                lastSelected = currentSelected;  // 現在の選択ボタンを記憶させる
            }
        }
    }
}
