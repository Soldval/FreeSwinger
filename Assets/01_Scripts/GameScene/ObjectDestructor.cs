// ObjectDestructor.cs

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
    [SerializeField] private float explosionForce = 10.0f;
    [SerializeField] private float chainUpwardForce = 10.0f;

    [Header("加算スコア設定")]
    [SerializeField] private int explodeScore = 2000;
    [SerializeField] private int chainBreakScore = 5000;

    // 内部変数
    private bool hasBeenHit = false;
    private bool isDestroyed = false;

    /// <summary>
    /// バットに打たれた時に呼び出す処理
    /// </summary>
    public void MarkAsHit()
    {
        AudioManager.PlaySEBatHit();
        hasBeenHit = true;
    }

    /// <summary>
    /// 飛ばされたオブジェクトが何かに衝突した時に爆破消滅させる処理
    /// </summary>
    /// <param name="collision">ぶつかった何か</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenHit || isDestroyed) return;

        // プレイヤー自身やバットに当たった場合は無視
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("bat")) return;

        if (GameScore.Instance != null)
        {
            GameScore.Instance.AddScore(explodeScore);
        }

        // 自分が何かに直撃した場合、爆発エフェクト処理を実行する。
        ExplodeAndChainBreak(false);
     }

    /// <summary>
    /// 爆発エフェクト実行と巻き込み消滅判定の処理
    /// </summary>
    /// <param name="isChained">連鎖状態(巻き込み)</param>
    public void ExplodeAndChainBreak(bool isChained)
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if(isChained)
        {
            Rigidbody rbSelf = GetComponent<Rigidbody>();
            if(rbSelf != null)
            {
                rbSelf.linearVelocity = Vector3.zero;
                rbSelf.AddForce(Vector3.up * chainUpwardForce, ForceMode.Impulse);
            }
        }

        // エフェクトの生成
        if (isChained && chainBreakEffectPrefab != null)
        {
            Instantiate(chainBreakEffectPrefab, transform.position, Quaternion.identity);
        }
        else if(!isChained && breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        // もし自身オブジェクトが直撃爆発ならば、周囲のオブジェクトをさらに巻き込んで飛ばし消滅させる。
        if(!isChained)
        {
            // ---- 爆風の範囲内にいる周囲オブジェクトを巻き込み飛ばして消滅させる
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
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 3.0f, ForceMode.Impulse);
                }

                // ObjectDestructorを持っているオブジェクトのみを連鎖破壊対象にする
                ObjectDestructor otherDestructor = hitCollider.GetComponent<ObjectDestructor>();
                if (otherDestructor != null)
                {
                    otherDestructor.ExplodeAndChainBreak(true);
                    Destroy(hitCollider.gameObject, 0.3f);
                    if (GameScore.Instance != null)
                    {
                        GameScore.Instance.AddScore(chainBreakScore);
                    }
                }
            }
        }

        if(!isChained)
        {
            AudioManager.PlaySEExplosion();
        }
        else
        {
            AudioManager.PlaySEChainBreak();
        }

        Destroy(gameObject);

        Debug.Log(gameObject.name + " が破壊されました(連鎖爆発: " + isChained + ")");
    }
}
