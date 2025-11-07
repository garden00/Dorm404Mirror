using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExample : MonoBehaviour, IDamageable
{
    [SerializeField]
    private GameObject projectlie;

    [SerializeField]
    private EightDirection throwDirection;

    [SerializeField]
    private float throwCycleTime;
    float throwTimer = 0;

    // 읽기 전용, 애니메이션 넣을 때 쓸 거예요!!
    public float ThrowCycleTime => throwCycleTime;

    [SerializeField]
    private int maxHealth = 100;
    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = value;

            if (currentHealth < 0)
            {
                currentHealth = 0;
            }
            else if (currentHealth > maxHealth) currentHealth = maxHealth;


            //Debug.Log("enemy health : " + currentHealth);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        ThrowCycle();

    }

    public void ReceiveAttack(IProjectile projectile)
    {
        CurrentHealth = CurrentHealth - projectile.Damage;
        return;
    }

    private void ThrowCycle()
    {
        throwTimer += Time.deltaTime;

        if (throwTimer > throwCycleTime)
        {
            throwTimer = 0f;
            ThrowProjectile();
        }
    }

    private void ThrowProjectile()
    {
        ObjectPoolingManager.Instance.GetPrefab(projectlie)
            .GetComponent<IProjectile>().
            Fire(transform.position, throwDirection, gameObject.tag);
    }
}
