// MiniMapCameraFollow.cs

using UnityEngine;

/// <summary>
/// ミニマップカメラの追従用クラス
/// </summary>
public class MiniMapCameraFollow : MonoBehaviour
{
    [Tooltip("プレイヤーオブジェクト")]
    public Transform player;

    [Tooltip("プレイヤーからの高さ(Y軸オフセット)")]
    public float height = 100f;

    void LateUpdate()
    {
        if (player == null) return;

        // プレイヤーに追従する(Y軸は固定オフセットのまま)
        Vector3 newPosition = player.position;
        newPosition.y += height;
        transform.position = newPosition;

        // カメラは真下を向いたまま
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
