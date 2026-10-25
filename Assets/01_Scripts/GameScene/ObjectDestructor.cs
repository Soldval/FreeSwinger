// ObjectDestructor.cs

using UnityEngine;

/// <summary>
/// バットで飛ばされたモノが何かに衝突したら消滅するクラス
/// </summary>
public class ObjectDestructor : MonoBehaviour
{
    [Header("生成するエフェクト")]
    [SerializeField] private GameObject breakEffectPrefab;  // バットで打たれて他に衝突した時のエフェクト
    [SerializeField] private GameObject chainBreakEffectPrefab;  // 巻き込まれたオブジェクトが消えるエフェクト

    [Header("巻き込みエフェクト設定")]
    [SerializeField] private float explosionRadius = 3.0f;
    [SerializeField] private float explosionForce = 10.0f;

    // 内部変数
    private bool hasBeenHit = false;
    private bool isDestroyed = false;

    /// <summary>
    /// バットに打たれた時に呼び出す処理
    /// </summary>
    public void MarkAsHit()
    {
        hasBeenHit = true;
    }

    /// <summary>
    /// 飛ばされたモノが何かに衝突した時に消滅させる処理
    /// </summary>
    /// <param name="collision">ぶつかった何か</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenHit || isDestroyed) return;

        // プレイヤー自身やバットに当たった場合は無視
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("bat")) return;

        // 自分が直撃して爆発した場合の処理を呼び出す
        Explode(false);
     }

    /// <summary>
    /// 爆発を実行する処理
    /// </summary>
    /// <param name="isChained"></param>
    public void Explode(bool isChained)
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // エフェクトの生成
        if (isChained && chainBreakEffectPrefab != null)
        {
            Instantiate(chainBreakEffectPrefab, transform.position, Quaternion.identity);
        }
        else if(!isChained && breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }

        // もし自分が直撃した側の爆発ならば、周囲のオブジェクトをさらに巻き込んで飛ばす
        if(!isChained)
        {
            // 爆風の範囲内にいる周囲オブジェクトを巻き込んで飛ばして消す
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hitCollider in colliders)
            {
                // 自分自身オブジェクトやプレイヤー、バットは巻き込み対象から除外
                if (hitCollider.gameObject == gameObject ||
                    hitCollider.gameObject.CompareTag("Player") ||
                    hitCollider.name.Contains("bat"))
                {
                    continue;
                }

                // 巻き込んだオブジェクトがRigidbodyを持っているか確認
                Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 外側に向かって力を加え吹き飛ばす
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1.0f, ForceMode.Impulse);
                }

                // ObjectDestructorを持っているオブジェクトだけを連鎖破壊の対象にする
                ObjectDestructor otherDestructor = hitCollider.GetComponent<ObjectDestructor>();
                if (otherDestructor != null)
                {
                    otherDestructor.Explode(true);
                    Destroy(hitCollider.gameObject, 0.3f);
                }
            }
        }
        // 自分自身を消滅させる
        Destroy(gameObject);

        Debug.Log(gameObject.name + " が破壊されました(連鎖爆発: " + isChained + ")");
    }
}
