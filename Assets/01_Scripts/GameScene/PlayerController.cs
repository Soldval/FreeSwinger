// PlayerController.cs

using UnityEngine;

/// <summary>
/// プレイヤー操作に関係するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;

    private CharacterController controller;

    void Start()
    {
        // 自分のCharacterControllerを取得
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // キーボードの入力を取得
        float horizontal = Input.GetAxis("Horizontal");  // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical");  // W/S or Up/Down

        // 移動方向を計算(カメラやワールドの向き基準)
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 入力がある場合、キャラクターを移動させて進行方向に向ける
        if(moveDirection.magnitude >= 0.1f)
        {
            // キャラクターを進行方向に向かせる
            transform.rotation = Quaternion.LookRotation(moveDirection);

            // 実際に移動させる(スピード × フレーム時間)
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        
    }
}
