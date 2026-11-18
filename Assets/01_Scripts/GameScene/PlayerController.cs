// PlayerController.cs

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using UnityEngine.Rendering;

/// <summary>
/// プレイヤー操作に関係するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移動速度設定用")]
    [SerializeField] private float normalMoveSpeed = 2.0f;
    [SerializeField] private float chargeMoveSpeed = 0.8f;
    private float normalSprintSpeed = 12.0f;

    [Header("チャージ設定用")]
    [SerializeField] private float chargeThreshold = 3.0f;
    private float chargeTimer = 0.0f;

    [Header("チャージ時バット位置調整用")]
    [SerializeField] private Vector3 chargeBatPosition = new Vector3(-0.083f, -1.359f, -0.104f);
    [SerializeField] private Vector3 chargeBatRotation = new Vector3(17.401f, -4.919f, -93.72f);

    [Header("スイング設定用")]
    [SerializeField] private Animator animator;
    public bool isSwinging = false;
    public bool isCharging = false;

    [Header("バット持ち替え設定用")]
    [SerializeField] private Transform batTransform;
    [SerializeField] private Transform batAnchorRight;  // 場所:胸ボーンの子オブジェクト

    [Header("ボーン参照用(動的持ち替え用)")]
    [SerializeField] private Transform chestBone;
    [SerializeField] private Transform rightHandBone;

    [Header("右手用 IK設定")]
    [SerializeField] private Transform rightHandTarget;
    [SerializeField][Range(0, 1)] private float rightIkWeight = 1.0f;

    [Header("左手・左肘用 IK設定")]
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform leftElbowHint;
    [SerializeField] private Transform chargeLeftHandTarget;
    [SerializeField] private Transform chargeLeftElbowHint;
    [SerializeField][Range(0, 1)] private float LeftIkWeight = 1.0f;

    [Header("バットヒット制御用")]
    [SerializeField] private BatHitController batHitController;

    [Header("トレイル設定")]
    [SerializeField] private TrailRenderer batTrailRendererTop;
    [SerializeField] private TrailRenderer batTrailRendererUnder;

    [Header("エフェクト設定")]
    [SerializeField] private EffectController effectController;

    // 内部変数(バットの初期位置・角度保存用変数)
    private Vector3 initialAnchorPosition;
    private Quaternion initialAnchorRotation;

    // 内部変数(その他)
    private ThirdPersonController thirdPersonController;
    private CharacterController characterController;
    private Coroutine autoEndSwingCoroutine;
    private float lockedGroundY;
    private bool canControl = true;
    private bool isLeftHandReleased = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        characterController = GetComponent<CharacterController>();

        if(thirdPersonController != null )
        {
            normalMoveSpeed = thirdPersonController.MoveSpeed;
            normalSprintSpeed = thirdPersonController.SprintSpeed;
        }

        // batAnchorRight(胸ボーン位置)から位置・角度を記憶する
        if(batAnchorRight != null)
        {
            initialAnchorPosition = batAnchorRight.localPosition;
            initialAnchorRotation = batAnchorRight.localRotation;
        }
    }

    void Update()
    {
        // プレイヤー操作可能/不可能を確認
        if (!canControl) return;

        if(isCharging)
        {
            chargeTimer += Time.deltaTime;

            if(effectController != null)
            {
                effectController.UpdateChargeProgress(chargeTimer, chargeThreshold);
            }
        }
    }

    private void OnDisable()
    {
        // シーン移動時に残っているコルーチンを強制停止させる(エラー防止用)
        StopAllCoroutines();
    }

    #region プレイヤー操作の可能/禁止にする処理 ----

    /// <summary>
    /// プレイヤー操作を可能にする処理
    /// </summary>
    public void EnableControl()
    {
        canControl = true;

        if(thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
        }
    }

    /// <summary>
    /// プレイヤー操作を禁止にする処理
    /// </summary>
    public void DisableControl()
    {
        canControl = false;

        // チャージ状態であれば、エフェクトクリアとチャージ解除を行う。
        if(isCharging)
        {
            isCharging = false;
            if(effectController != null)
            {
                effectController.StopAllEffects();
            }
            if(animator != null)
            {
                animator.SetBool("IsCharging", false);
            }

            if(thirdPersonController != null)
            {
                thirdPersonController.MoveSpeed = normalMoveSpeed;
                thirdPersonController.SprintSpeed = normalSprintSpeed;
            }
        }

        if(thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }
    }

    #endregion // プレイヤー操作の可能/禁止にする処理 ----

    #region チャージ関係の処理 ----

    /// <summary>
    /// InputSystemのChargeで呼び出される処理(InvokeUnityEvents)
    /// </summary>
    /// <param name="context">入力状況</param>
    public void OnCharge(InputAction.CallbackContext context)
    {
        if (!canControl) return;

        if(context.started && !isSwinging && !isCharging)
        {
            StartCharge();
        }
        else if(context.canceled && isCharging)
        {
            StartSwing();
        }
    }

    /// <summary>
    /// チャージ状態の開始処理
    /// </summary>
    public void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0.0f;

        lockedGroundY = transform.position.y;

        if(thirdPersonController != null)
        {
            thirdPersonController.MoveSpeed = chargeMoveSpeed;
            thirdPersonController.SprintSpeed = chargeMoveSpeed;
        }

        if(effectController != null)
        {
            effectController.StartChargeEffect();
        }

        if(animator != null)
        {
            animator.ResetTrigger("LeftSwingTrigger");
            animator.ResetTrigger("ChargeLeftSwingTrigger");
            animator.SetBool("IsCharging", true);
        }

        // チャージ開始でバットを胸ボーン(チャージポジション)に追従させる
        if (batAnchorRight != null && rightHandBone != null)
        {
            batAnchorRight.SetParent(rightHandBone, false);
            batAnchorRight.localPosition = chargeBatPosition;
            batAnchorRight.localRotation = Quaternion.Euler(chargeBatRotation);
        }

        // チャージアニメーション時、右手はアニメーションに合わせ左手はIKでバットをしっかり持つ。
        DisableRightHandIK();
        EnableLeftHandIK();
    }

    #endregion // チャージ関係の処理 ----

    #region スイング関係の処理 ----

    /// <summary>
    /// スイング動作の開始処理
    /// </summary>
    public void StartSwing()
    {
        isCharging = false;
        isSwinging = true;
        isLeftHandReleased = false;
        bool isMaxCharged = (chargeTimer >= chargeThreshold);

        // スイング中は移動を止める
        if(thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }

        // スイング中はCharacterControllerを無効化し足の位置を地面に固定する
        if(characterController != null)
        {
            characterController.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("IsCharging", false);
        }

        // バットコライダーONは少し遅らせるためアニメーションのイベントでONにさせる
       //if(batHitController != null)
       // {
       //     batHitController.EnableCollider(isMaxCharged);
       // }
        
        if(effectController != null)
        {
            effectController.StartSwingEffect(isMaxCharged);
        }

        // スイング開始時に両手のIKをOFFにする
        DisableRightHandIK();
        EnableLeftHandIK();

        // 古いコルーチンが動いていたらスイングを強制停止させる
        if(autoEndSwingCoroutine != null)
        {
            StopCoroutine(autoEndSwingCoroutine);
        }

        // マックスチャージならチャージスイング、通常チャージなら通常スイングを行う。
        if(chargeTimer >= chargeThreshold)
        {
            animator.SetTrigger("ChargeLeftSwingTrigger");
            autoEndSwingCoroutine = StartCoroutine(AutoEndSwingRoutine(2.0f));
        }
        else
        {
            animator.SetTrigger("LeftSwingTrigger");
            autoEndSwingCoroutine = StartCoroutine(AutoEndSwingRoutine(1.2f));
        }
    }

    /// <summary>
    /// スイング動作の終了処理
    /// </summary>
    public void EndSwing()
    {
        Debug.Log("EndSwing実行");

        isSwinging = false;
        isCharging = false;
        isLeftHandReleased = false;

        if(batHitController != null)
        {
            batHitController.DisableCollider();
        }

        // 古いコルーチンが動いていれば強制停止させ空にする
        if(autoEndSwingCoroutine != null)
        {
            StopCoroutine(autoEndSwingCoroutine);
            autoEndSwingCoroutine = null;
        }

        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
            thirdPersonController.MoveSpeed = normalMoveSpeed;
            thirdPersonController.SprintSpeed = normalSprintSpeed;
        }

        if(characterController != null)
        {
            characterController.enabled = true;
        }

        if(effectController != null)
        {
            effectController.StopAllEffects();
        }

        // スイング終了時に両手のIKをONにする
        EnableRightHandIK();
        EnableLeftHandIK();

        ResetBatTransform();
    }

    #endregion // スイング関係の処理 ----

    #region IK処理まとめ ----

    /// <summary>
    /// 右手のバット吸着をONにする処理
    /// </summary>
    public void EnableRightHandIK()
    {
        rightIkWeight = 1.0f;
    }

    /// <summary>
    /// 右手のバット吸着をOFFにする処理
    /// </summary>
    public void DisableRightHandIK()
    {
        rightIkWeight = 0.0f;
    }

    /// <summary>
    /// 左手のバット吸着をONにする処理
    /// </summary>
    public void EnableLeftHandIK()
    {
        LeftIkWeight = 1.0f;
    }

    /// <summary>
    /// 左手のバット吸着をOFFにする処理
    /// </summary>
    public void DisableLeftHandIK()
    {
        LeftIkWeight = 0.0f;
    }

    /// <summary>
    /// 左手・右手をTargetオブジェクトに吸着させる処理
    /// IK Pass が有効な時に自動呼び出し
    /// </summary>
    /// <param name="layerIndex">アニメーションレイヤーの番号</param>
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // ---- 右手のIK処理
        if (rightHandTarget != null)
        {
            // 位置と回転の吸着設定
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightIkWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightIkWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }

        bool shouldUseChargeTarget = isCharging || (isSwinging && !isLeftHandReleased);

        // ---- チャージ中ならばチャージ用ターゲットを使い、通常時は通常ターゲットを使う。
        Transform targetHand = (shouldUseChargeTarget && chargeLeftHandTarget != null) ? chargeLeftHandTarget : leftHandTarget;
        Transform targetElbow = (shouldUseChargeTarget && chargeLeftElbowHint != null) ? chargeLeftElbowHint : leftElbowHint;

        // ---- 左手のIK処理
        if (targetHand != null)
        {
            // 位置と回転の吸着設定
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, LeftIkWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, LeftIkWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, targetHand.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, targetHand.rotation);

            // 肘の曲がる方向制御
            if (targetElbow != null)
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, LeftIkWeight);
                animator.SetIKHintPosition(AvatarIKHint.LeftElbow, targetElbow.position);
            }
        }
        else
        {
            // ターゲットがない場合はIKを無しにする
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
        }
    }

    #endregion // IK処理まとめ ----

    #region その他処理まとめ ----

    /// <summary>
    /// バットの位置・角度を初期状態にリセットする処理
    /// </summary>
    public void ResetBatTransform()
    {
        if (batAnchorRight == null || chestBone == null) return;

        batAnchorRight.SetParent(chestBone, false);
        batAnchorRight.localPosition = initialAnchorPosition;
        batAnchorRight.localRotation = initialAnchorRotation;

        EnableRightHandIK();

        if(animator != null)
        {
            animator.SetBool("IsCharging", false);
        }
    }

    /// <summary>
    /// [アニメーションイベント用] バットコライダーを有効化する処理
    /// </summary>
    public void EnableColliderByAnimation()
    {
        bool isMaxCharged = (chargeTimer >= chargeThreshold);
        if (batHitController != null)
        {
            batHitController.EnableCollider(isMaxCharged);
            Debug.Log("【アニメーションイベント】コライダーをONにしました (最大チャージ: " + isMaxCharged + ")");
        }
    }

    /// <summary>
    /// [アニメーションイベント用] バットを右手ボーンの子オブジェクトにする処理
    /// </summary>
    public void AttachBatToRightHand()
    {
        if(batAnchorRight != null && rightHandBone != null)
        {
            batAnchorRight.SetParent(rightHandBone, true);
        }
    }

    /// <summary>
    /// [アニメーションイベント用] バットを胸ボーンの子オブジェクトにする処理
    /// </summary>
    public void AttachBatToChest()
    {
        if(batAnchorRight != null && chestBone != null)
        {
            batAnchorRight.SetParent(chestBone, true);
        }
    }

    /// <summary>
    /// [アニメーションイベント用] スイング途中で左手をバットから離す処理
    /// </summary>
    public void ReleaseLeftHand()
    {
        isLeftHandReleased = true;
        DisableLeftHandIK();
    }

    public void FadeOutLeftHandIK()
    {
        isLeftHandReleased = true;
        LeftIkWeight = 0.0f;
    }

    /// <summary>
    /// アニメーションイベントが実行されなかった場合の自動スイング解除処理(コルーチン)
    /// </summary>
    /// <param name="delaySeconds">ディレイ時間</param>
    /// <returns>待機時間 IEnumerator</returns>
    private IEnumerator AutoEndSwingRoutine(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        if (isSwinging)
        {
            Debug.Log("自動機能でEndSwingを実施しました");
            EndSwing();
        }
    }

    #endregion // その他処理まとめ ----
}
