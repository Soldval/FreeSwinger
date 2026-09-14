// BatSwing.cs

using UnityEngine;

public class BatSwing : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // キャラクターのAnimatorコンポーネントを取得
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Spaceキーが押されたらLeftSwingTriggerを実行
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(animator != null)
            {
                animator.SetTrigger("LeftSwingTrigger");
            }
        }
    }
}
