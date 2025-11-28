using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [SerializeField] int targetCount = 5; // n¸í
    int deadCount = 0;

    public System.Action OnAllEnemiesDead;

    void Awake()
    {
        Instance = this;
    }

    public void NotifyEnemyDead()
    {
        deadCount++;
        if (deadCount >= targetCount)
        {
            OnAllEnemiesDead?.Invoke();
        }
    }
}
