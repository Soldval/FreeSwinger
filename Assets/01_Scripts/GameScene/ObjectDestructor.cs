// ObjectDestructor.cs

using System;
using UnityEngine;

/// <summary>
/// バットで飛ばされたオブジェクトに関係するクラス
/// </summary>
public class ObjectDestructor : MonoBehaviour
{
    [Header("生成するエフェクト")]
    [SerializeField] private GameObject breakEffectPrefab;  // バットで打たれて他に衝突した時のエフェクト
    [SerializeField] private GameObject chainBreakEffectPrefab;  // 巻き込まれたオブジェクトが消えるエフェクト

    [Header("巻き込みエフェクト設定")]
    [SerializeField] private float explosionRadius = 3.0f;
    [SerializeField] private float explosionForce = 5.0f;
    [SerializeField] private float explosionUpwardModifier = 3.0f;
    [SerializeField] private float chainUpwardForce = 5.0f;

    [Header("加算スコア設定")]
    [SerializeField] private int explodeScore = 1000;
    [SerializeField] private int chainBreakScore = 4000;

    // 内部変数
    private bool hasBeenHit = false;
    private bool isDestroyed = false;
    private bool isChained = false;

    /// <summary>
    /// バットに打たれた時に呼び出す処理
    /// </summary>
    public void MarkAsHit()
    {
        AudioManager.PlaySEBatHit();
        hasBeenHit = true;
        isChained = false;
    }

    /// <summary>
    /// 飛ばされたオブジェクトが何かに衝突した時、爆破消滅と連鎖消滅させる処理。
    /// </summary>
    /// <param name="collision">ぶつかった何か</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenHit || isDestroyed) return;

        // プレイヤー自身やバットに当たった場合は無視
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bat")) return;

        isDestroyed = true;

        if (!isChained)  // ---- 直撃爆発の処理
        {
            if(GameScore.Instance != null)
            {
                GameScore.Instance.AddScore(explodeScore);
            }
            if(breakEffectPrefab != null)
            {
                Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
            }

            // ---- 爆風の範囲内にいる周囲オブジェクトを巻き込む
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hitCollider in colliders)
            {
                // 自分自身オブジェクト, プレイヤー, バットは巻き込み対象外
                if (hitCollider.gameObject == gameObject ||
                    hitCollider.gameObject.CompareTag("Player") ||
                    hitCollider.name.Contains("bat"))
                {
                    continue;
                }

                // 巻き込んだオブジェクトがRigidbodyを持っているか確認
                // 持っていれば外側方向に飛ばす
                Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, explosionUpwardModifier, ForceMode.Impulse);
                }

                // ObjectDestructorを持っているオブジェクトのみを連鎖破壊対象にする
                ObjectDestructor otherDestructor = hitCollider.GetComponent<ObjectDestructor>();
                if (otherDestructor != null)
                {
                    otherDestructor.BlastChainedObject(true);
                }
            }
            AudioManager.PlaySEExplosion();
        }
        else // ---- 連鎖爆発の処理
        {
            if(chainBreakEffectPrefab != null)
            {
                Instantiate(chainBreakEffectPrefab, transform.position, Quaternion.identity);
            }
            if (GameScore.Instance != null)
            {
                GameScore.Instance.AddScore(chainBreakScore);
            }
            AudioManager.PlaySEChainBreak();
        }

        Destroy(gameObject);
        Debug.Log(gameObject.name + " が破壊されました(連鎖爆発: " + isChained + ")");
    }

    /// <summary>
    /// 巻き込まれたオブジェクトを飛ばす処理
    /// </summary>
    /// <param name="chained">巻き込まれ状態</param>
    public void BlastChainedObject(bool chained)
    {
        if (isDestroyed || hasBeenHit) return;

        hasBeenHit = true;
        isChained = chained;

        if(isChained)
        {
            Rigidbody rbSelf = GetComponent<Rigidbody>();
            if(rbSelf != null)
            {
                rbSelf.linearVelocity = Vector3.zero;
                rbSelf.AddForce(Vector3.up * chainUpwardForce, ForceMode.Impulse);
            }
        }
    }
}
