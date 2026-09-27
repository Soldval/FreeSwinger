// PlayerController.cs

using UnityEngine;
using StarterAssets;

/// <summary>
/// プレイヤー操作に関係するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("スイング設定用")]
    [SerializeField] private Animator animator;
    public bool isSwinging = false;

    [Header("バット持ち替え設定用")]
    [SerializeField] private Transform batTransform;
    [SerializeField] private Transform batAnchorRight;

    [Header("バット両手持ち IK設定")]
    [SerializeField] private Transform leftHandTarget;
    [SerializeField][Range(0, 1)] private float ikWeight = 1.0f;  // 0:左手はフリー 1:左手はバットを持つ

    [Header("左手ボーンの参照用")]
    [SerializeField] private Transform leftHandBone;

    [Header("右手用 IK設定")]
    [SerializeField] private Transform rightHandTarget;
    [SerializeField][Range(0, 1)] private float rightIkWeight = 1.0f;

    // バットの初期位置・角度保存用変数
    private Vector3 initialBatPosition;
    private Quaternion initialBatRotation;

    // その他内部変数
    private ThirdPersonController thirdPersonController;

    void Start()
    {
        animator = GetComponent<Animator>();
        thirdPersonController = GetComponent<ThirdPersonController>();

        // バットの初期位置・角度を記憶しておく
        if(batTransform != null)
        {
            initialBatPosition = batTransform.localPosition;
            initialBatRotation = batTransform.localRotation;
        }
    }

    void Update()
    {
        // Spaceキー押下でスイング動作開始
        if(Input.GetKeyDown(KeyCode.Space) && !isSwinging)
        {
            StartSwing();
        }
    }

    /// <summary>
    /// 左手をLeftHandTargetオブジェクトに吸着させる処理
    /// IK Pass が有効な時に自動呼び出し
    /// </summary>
    /// <param name="layerIndex"></param>
    private void OnAnimatorIK(int layerIndex)
    {
        Debug.Log($"IK処理実行中 - ikWeight: {ikWeight}, target: {leftHandTarget}");

        if (animator == null) return;

        // 右手のIK処理
        if(rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightIkWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightIkWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }

        // 左手のIK処理
        if (leftHandTarget != null)
        {
            // 位置の吸着
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);

            // 回転の吸着
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
        //else
        //{
        //    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
        //    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        //}
    }

    void LateUpdate()
    {
        if (leftHandTarget != null && leftHandBone != null && ikWeight > 0.5f)
        {
            leftHandBone.position = leftHandTarget.position;
            leftHandBone.rotation = leftHandTarget.rotation;
        }
    }

    /// <summary>
    /// スイング動作の開始処理
    /// </summary>
    public void StartSwing()
    {
        isSwinging = true;

        // ThirdPersonControllerを無効化する
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }

        EnableLeftHandIK();  // スイング開始時は両手持ち(IK有効)

        animator.SetTrigger("LeftSwingTrigger");
    }

    /// <summary>
    /// スイング動作の終了処理
    /// </summary>
    public void EndSwing()
    {
        isSwinging = false;

        // ThirdPersonControllerを有効にする
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
        }

        // バット構え状態に戻ったら、両手持ちにしバットを初期状態にする。
        EnableLeftHandIK();
        ResetBatTransform();
    }

    /// <summary>
    /// 左手のバット吸着をONにする処理
    /// </summary>
    public void EnableLeftHandIK()
    {
        ikWeight = 1.0f;
    }

    /// <summary>
    /// 左手のバット吸着をOFFにする処理
    /// </summary>
    public void DisableLeftHandIK()
    {
        ikWeight = 0.0f;
    }    

    /// <summary>
    /// バットの位置・角度を初期状態にリセットする処理
    /// </summary>
    public void ResetBatTransform()
    {
        if (batTransform == null || batAnchorRight == null) return;

        batTransform.SetParent(rightHandTarget);
        batTransform.localPosition = initialBatPosition;
        batTransform.localRotation = initialBatRotation;
    }
}
