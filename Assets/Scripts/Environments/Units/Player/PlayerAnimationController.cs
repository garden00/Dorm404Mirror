using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerDirectionAnimator : MonoBehaviour
{
    private Animator animator;
    private Vector2 input;
    private Vector2 lastLook;

    void Awake()
    {
        animator = GetComponent<Animator>();
        lastLook = Vector2.down;
        animator.SetFloat("lastX", lastLook.x);
        animator.SetFloat("lastY", lastLook.y);
    }

    void Update()
    {
        // 화살표 키로
        input.x = (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) +
                  (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0);
        input.y = (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) +
                  (Input.GetKey(KeyCode.DownArrow) ? -1 : 0);

        animator.SetFloat("moveX", input.x);
        animator.SetFloat("moveY", input.y);

        // 방향 전환
        if (Input.GetKeyDown(KeyCode.UpArrow))
            lastLook = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            lastLook = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            lastLook = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            lastLook = Vector2.right;

        // 마지막 방향을 Animator에 반영
        animator.SetFloat("lastX", lastLook.x);
        animator.SetFloat("lastY", lastLook.y);
    }
}
