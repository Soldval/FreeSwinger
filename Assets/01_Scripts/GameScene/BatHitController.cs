// BatHitController.cs

using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// バットヒットの衝突用クラス
/// </summary>
public class BatHitController : MonoBehaviour
{
    [Header("バットの吹っ飛ばす強さ")]
    [SerializeField] private float hitPower = 15.0f;
    [SerializeField] private float upwardPower = 5.0f;

    [Header("加算スコア設定")]
    [SerializeField] private int hitScore = 100;

    /// <summary>
    /// バットのcolliderが他オブジェクトに衝突した瞬間に呼ばれる処理
    /// </summary>
    /// <param name="collision">衝突した側のcollision</param>
    private void OnCollisionEnter(Collision collision)
    {
        // 衝突した側のオブジェクトがRigidbodyを持っているか確認する
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb == null) return;

        // 衝突した地点から前方へ力を加える
        Vector3 hitDirection = (collision.transform.position - transform.position).normalized;

        // 前方への力に上方向の力を加える
        Vector3 finalForce = (hitDirection * hitPower) + (Vector3.up * upwardPower);

        // 力を加えてオブジェクトを吹っ飛ばす
        rb.AddForce(finalForce, ForceMode.Impulse);

        // スコア加算
        if(GameScore.Instance != null)
        {
            GameScore.Instance.AddScore(hitScore);
        }

        Debug.Log(collision.gameObject.name + "を打ち飛ばしました！");
    }
}
