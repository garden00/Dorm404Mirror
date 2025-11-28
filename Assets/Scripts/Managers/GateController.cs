using UnityEngine;

public class GateController : MonoBehaviour
{
    [SerializeField] Collider2D gateCollider;
    [SerializeField] Animator gateAnimator;

    void Start()
    {
        // 에너미 모두 죽으면 자동 실행
        EnemyManager.Instance.OnAllEnemiesDead += OpenGate;
    }

    void OpenGate()
    {
        gateCollider.enabled = false;

        if (gateAnimator != null)
            gateAnimator.SetTrigger("Open");
    }

    void OnDestroy()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.OnAllEnemiesDead -= OpenGate;
    }

}
