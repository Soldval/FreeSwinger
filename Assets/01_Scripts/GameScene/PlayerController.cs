// PlayerController.cs

using System.Collections;
using UnityEngine;
using StarterAssets;

/// <summary>
/// プレイヤー操作に関係するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("移動速度設定用")]
    [SerializeField] private float normalMoveSpeed = 2.0f;
    [SerializeField] private float chargeMoveSpeed = 0.8f;

    [Header("スイング設定用")]
    [SerializeField] private Animator animator;
    public bool isSwinging = false;
    public bool isCharging = false;

    [Header("チャージ設定用")]
    [SerializeField] private float chargeThreshold = 3.0f;
    private float chargeTimer = 0.0f;

    [Header("チャージ時バット位置調整用")]
    [SerializeField] private Vector3 chargeBatPosition = new Vector3(-0.083f, -1.359f, -0.104f);
    [SerializeField] private Vector3 chargeBatRotation = new Vector3(17.401f, -4.919f, -93.72f);     

    [Header("バット持ち替え設定用")]
    [SerializeField] private Transform batTransform;
    [SerializeField] private Transform batAnchorRight;

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
    //[SerializeField] private BatTrailMesh batTrailMesh;
    [SerializeField] private TrailRenderer batTrailRendererTop;
    [SerializeField] private TrailRenderer batTrailRendererUnder;

    [Header("エフェクト設定")]
    [SerializeField] private EffectController effectController;

    // バットの初期位置・角度保存用変数
    private Vector3 initialAnchorPosition;
    private Quaternion initialAnchorRotation;

    // その他内部変数
    private ThirdPersonController thirdPersonController;
    private CharacterController characterController;
    private Coroutine autoEndSwingCoroutine;
    private float lockedGroundY;
    private bool canControl = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        characterController = GetComponent<CharacterController>();

        // 通常速度を初期設定から記録
        if(thirdPersonController != null )
        {
            normalMoveSpeed = thirdPersonController.MoveSpeed;
        }

        // BatAnchor_rの胸から相対位置・角度を記憶する
        if(batAnchorRight != null)
        {
            initialAnchorPosition = batAnchorRight.localPosition;
            initialAnchorRotation = batAnchorRight.localRotation;
        }

        // 初期状態でBatAnchor_rを胸ボーン(chestBone)の子にする
        if(batAnchorRight != null && chestBone != null)
        {
            batAnchorRight.SetParent(chestBone, false);
            batAnchorRight.localPosition = initialAnchorPosition;
            batAnchorRight.localRotation = initialAnchorRotation;
        }
    }

    void Update()
    {
        if (!canControl) return;

        // Spaceキー押し始めたら構え状態に入る
        if(Input.GetKeyDown(KeyCode.Space) && !isSwinging && !isCharging)
        {
            StartCharge();
        }

        // チャージ中はタイマーをカウントアップ
        if(isCharging)
        {
            chargeTimer += Time.deltaTime;

            // チャージ中のエフェクト進行度を更新
            if(effectController != null)
            {
                effectController.UpdateChargeProgress(chargeTimer, chargeThreshold);
            }
        }

        // Spaceキーを離し瞬間にスイングする
        if(Input.GetKeyUp(KeyCode.Space) && isCharging)
        {
            StartSwing();
        }
    }

    void LateUpdate()
    {
        if(isSwinging)
        {
            Vector3 pos = transform.position;
            pos.y = lockedGroundY;
            transform.position = pos;
        }
    }

    public void EnableControl()
    {
        canControl = true;

        if(thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
        }
    }

    /// <summary>
    /// プレイヤー操作を一切禁止にする処理
    /// </summary>
    public void DisableControl()
    {
        canControl = false;

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
        }

        if(thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }
    }

    /// <summary>
    /// Spaceキー長押しによる構え状態の開始処理
    /// </summary>
    public void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0.0f;

        lockedGroundY = transform.position.y;

        // 移動速度をチャージ用低速にする
        if(thirdPersonController != null)
        {
            thirdPersonController.MoveSpeed = chargeMoveSpeed;
        }

        // チャージ開始のエフェクトを呼び出す
        if(effectController != null)
        {
            effectController.StartChargeEffect();
        }

        // 残っているスイングTriggerをクリアする
        if(animator != null)
        {
            animator.ResetTrigger("LeftSwingTrigger");
            animator.ResetTrigger("ChargeLeftSwingTrigger");
            animator.SetBool("IsCharging", true);
        }

        // チャージ開始時点でバットを右手ボーンに追従させる
        if(batAnchorRight != null && chestBone != null)
        {
            batAnchorRight.SetParent(chestBone, false);
            batAnchorRight.localPosition = chargeBatPosition;
            batAnchorRight.localRotation = Quaternion.Euler(chargeBatRotation);
        }

        // チャージアニメーション時、右手はアニメーションに合わせ左手はIKでバットをしっかり持つ。
        DisableRightHandIK();
        EnableLeftHandIK();
    }

    /// <summary>
    /// スイング動作の開始処理
    /// </summary>
    public void StartSwing()
    {
        isCharging = false;
        isSwinging = true;
        bool isMaxCharged = (chargeTimer >= chargeThreshold);

        AudioManager.PlaySELeftSwing();

        // 振りぬいている最中は移動を止める
        if(thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }

        // スイング中はCharacterControllerも無効化して足の位置を地面に固定する
        if(characterController != null)
        {
            characterController.enabled = false;
        }

        // チャージ状態を解除する
        if (animator != null)
        {
            animator.SetBool("IsCharging", false);
        }

        // バットのコライダーをONにする
        if(batHitController != null)
        {
            batHitController.EnableCollider(isMaxCharged);
        }
        
        // EffectController側のスイングエフェクト開始メソッドを呼び出す
        if(effectController != null)
        {
            //bool isMaxCharge = (chargeTimer >= chargeThreshold);
            effectController.StartSwingEffect(isMaxCharged);
        }

        // スイング開始時に両手のIKをOFFにする
        DisableRightHandIK();
        DisableLeftHandIK();

        // 既存のタイマーがあれば停止
        if(autoEndSwingCoroutine != null)
        {
            StopCoroutine(autoEndSwingCoroutine);
        }

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

        // バットのコライダーをOFFにする
        if(batHitController != null)
        {
            batHitController.DisableCollider();
        }

        // コルーチンが走っていれば停止
        if(autoEndSwingCoroutine != null)
        {
            StopCoroutine(autoEndSwingCoroutine);
            autoEndSwingCoroutine = null;
        }

        // 移動速度を通常速度に戻す
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
            thirdPersonController.MoveSpeed = normalMoveSpeed;
        }

        // CharacterControllerを再有効化する
        if(characterController != null)
        {
            characterController.enabled = true;
        }

        // エフェクトを全て停止・消去する
        if(effectController != null)
        {
            effectController.StopAllCoroutines();
        }

        // スイング終了時に両手のIKをONにする
        EnableRightHandIK();
        EnableLeftHandIK();

        ResetBatTransform();
    }

    /// <summary>
    /// アニメーションイベントが打たれていなかった場合の自動安全解除
    /// </summary>
    /// <param name="delaySeconds"></param>
    /// <returns></returns>
    private IEnumerator AutoEndSwingRoutine(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);

        if(isSwinging)
        {
            Debug.Log("自動安全機能でEndSwingを実施しました");
            EndSwing();
        }
    }

    /// <summary>
    /// 左手・右手をTargetオブジェクトに吸着させる処理
    /// IK Pass が有効な時に自動呼び出し
    /// </summary>
    /// <param name="layerIndex"></param>
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // 右手のIK処理
        if(rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightIkWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightIkWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }

        // チャージ中ならばチャージ用ターゲットを使い、通常時は通常ターゲットを使う。
        Transform targetHand = (isCharging && chargeLeftHandTarget != null) ? chargeLeftHandTarget : leftHandTarget;
        Transform targetElbow = (isCharging && chargeLeftElbowHint != null) ? chargeLeftElbowHint : leftElbowHint;

        // 左手のIK処理
        if (targetHand != null)
        {
            // 位置と回転の吸着度合
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
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
        }
    }

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
    /// バットの位置・角度を初期状態にリセットする処理
    /// </summary>
    public void ResetBatTransform()
    {
        if (batAnchorRight == null || chestBone == null) return;

        batAnchorRight.SetParent(chestBone, false);
        batAnchorRight.localPosition = initialAnchorPosition;
        batAnchorRight.localRotation = initialAnchorRotation;

        if(animator != null)
        {
            animator.SetBool("IsCharging", false);
        }
    }

    /// <summary>
    /// [アニメーションイベント用] バットを右手ボーンの子にする処理
    /// </summary>
    public void AttachBatToRightHand()
    {
        if(batAnchorRight != null && rightHandBone != null)
        {
            batAnchorRight.SetParent(rightHandBone, true);
        }
    }

    public void AttachBatToChest()
    {
        if(batAnchorRight != null && chestBone != null)
        {
            batAnchorRight.SetParent(chestBone, true);
        }
    }
}
