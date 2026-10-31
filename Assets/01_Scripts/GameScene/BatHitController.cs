// BatHitController.cs

using UnityEngine;

/// <summary>
/// バットヒットの衝突用クラス
/// </summary>
public class BatHitController : MonoBehaviour
{
    [Header("バットの吹っ飛ばす強さ")]
    [SerializeField] private float hitPower = 15.0f;
    [SerializeField] private float upwardPower = 15.0f;

    [Header("バットのコライダー設定")]
    [SerializeField] private Collider leftSwingCollider;
    [SerializeField] private Collider chargeLeftSwingCollider;

    [Header("加算スコア設定")]
    [SerializeField] private int hitScore = 1000;

    void Awake()
    {
        // バットのコライダーをOFFにする
        DisableCollider();
    }

    /// <summary>
    /// バットのコライダーを有効化する処理
    /// </summary>
    /// <param name="isMaxCharged">最大チャージ状態</param>
    public void EnableCollider(bool isMaxCharged)
    {
        if(isMaxCharged)
        {
            // 最大チャージ時はチャージスイングコライダーをON
            if(chargeLeftSwingCollider != null)
            {
                chargeLeftSwingCollider.enabled = true;
            }
            if(leftSwingCollider != null)
            {
                leftSwingCollider.enabled = false;
            }
        }
        else
        {
            // 通常チャージ時は通常スイングコライダーをON
            if (chargeLeftSwingCollider != null)
            {
                chargeLeftSwingCollider.enabled = false;
            }
            if (leftSwingCollider != null)
            {
                leftSwingCollider.enabled = true;
            }
        }
    }

    /// <summary>
    /// バットのコライダーを無効化する処理
    /// </summary>
    public void DisableCollider()
    {
        // チャージスイング・通常スイングのコライダー両方をOFF
        if(chargeLeftSwingCollider != null)
        {
            chargeLeftSwingCollider.enabled = false;
        }
        if (leftSwingCollider != null)
        {
            leftSwingCollider.enabled = false;
        }
    }

    /// <summary>
    /// バットのコライダーが他オブジェクトに衝突した瞬間に呼ばれる処理
    /// </summary>
    /// <param name="collision">衝突した側のコリジョン</param>
    private void OnCollisionEnter(Collision collision)
    {
        // 衝突した側がRigidbodyを持っているか確認する
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
        if (rb == null) return;

        // ObjectDestructorが付いている場合、打たれた処理を行う。
        ObjectDestructor destructor = collision.gameObject.GetComponent<ObjectDestructor>();
        if(destructor != null)
        {
            destructor.MarkAsHit();
        }

        // 衝突した地点から前方へ力を加える
        Vector3 hitDirection = (collision.transform.position - transform.position).normalized;

        // 前方への力に上方向の力を加える
        Vector3 finalForce = (hitDirection * hitPower) + (Vector3.up * upwardPower);

        // 力を加えてオブジェクトを吹っ飛ばす
        rb.AddForce(finalForce, ForceMode.Impulse);

        // 打たれたオブジェクトにランダムな回転を与える
        Vector3 randomtorque = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f)) * 50f;
        rb.AddTorque(randomtorque, ForceMode.Impulse);

        // スコア加算する
        if(GameScore.Instance != null)
        {
            GameScore.Instance.AddScore(hitScore);
        }

        Debug.Log(collision.gameObject.name + "を打ち飛ばしました！");
    }
}
