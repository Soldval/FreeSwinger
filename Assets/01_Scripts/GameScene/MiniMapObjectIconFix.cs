// MiniMapObjectIconFix.cs

using UnityEngine;

/// <summary>
/// ミニマップのオブジェクトアイコン回転固定用クラス
/// </summary>
public class MiniMapObjectIconFix : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
