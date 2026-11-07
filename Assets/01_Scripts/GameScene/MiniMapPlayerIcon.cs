// MiniMapPlayerIcon.cs

using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ミニマップのプレイヤーアイコン追従・回転考慮用クラス
/// </summary>
public class MiniMapPlayerIcon : MonoBehaviour
{
    [Header("操作キャラのTransform")]
    [SerializeField] private Transform playerTransform;

    void Update()
    {
        if (playerTransform == null) return;

        // プレイヤーのY軸回転をそのままUIのZ軸回転に反映する
        Vector3 rot = transform.eulerAngles;
        rot.z = -playerTransform.eulerAngles.y;
        transform.eulerAngles = rot;
    }
}
