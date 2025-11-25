using UnityEngine;

public class GateOpenTrigger : MonoBehaviour
{
    [SerializeField] private Animator gateAnimator;
    [SerializeField] private Collider2D gateCollider; // 문 콜라이더
    private bool bossDead = false;
    private bool opened = false;

    public void SetBossDead()
    {
        bossDead = true;
    }

    // 플레이어가 트리거 영역에 들어왔을 때 자동으로 호출됨
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!bossDead) return;
        if (!other.CompareTag("Player")) return;
        if (opened) return;

        gateAnimator.SetTrigger("Open");

        if (gateCollider != null)
            gateCollider.enabled = false;
    }
}
