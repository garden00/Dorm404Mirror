using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [SerializeField] bool isBossStage = false;
    [SerializeField] int targetCount = 5;
    [SerializeField] GameObject boss;   // 보스 오브젝트 직접 Drag&Drop

    int deadCount = 0;

    public System.Action OnAllEnemiesDead;

    void Awake()
    {
        Instance = this;
    }

    // === 일반 몬스터 죽음 ===
    public void NotifyEnemyDead()
    {
        if (isBossStage) return;

        deadCount++;
        if (deadCount >= targetCount)
            OnAllEnemiesDead?.Invoke();
    }

    // === 보스 죽음 ===
    public void NotifyBossDead()
    {
        if (!isBossStage) return;

        OnAllEnemiesDead?.Invoke();
    }
}
