using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCombat : MonoBehaviour, IDamageable
{
    [NonSerialized]
    public PlayerStatus status;

    [SerializeField]
    private float invincibleTime;//무적시간
    private float invincibleTimer;

    [SerializeField]
    private GameObject ChargingAttackProjectilePrefab;


    // 방패 스프라이트 (애니메이션 필요 x)
    [Header("Shield Sprites (8-way)")]
    [SerializeField] private Sprite shieldUp;
    [SerializeField] private Sprite shieldDown;
    [SerializeField] private Sprite shieldLeft;
    [SerializeField] private Sprite shieldRight;
    [SerializeField] private Sprite shieldUpLeft;
    [SerializeField] private Sprite shieldUpRight;
    [SerializeField] private Sprite shieldDownLeft;
    [SerializeField] private Sprite shieldDownRight;

    // 방패 오브젝트 및 렌더러
    [SerializeField] private GameObject ShildObject;
    private SpriteRenderer shieldSR;

    private Vector3 shildDirection;
    private bool isCharging;

    void Awake()
    {
        if (ShildObject != null) shieldSR = ShildObject.GetComponent<SpriteRenderer>();
    }


    private void Update()
    {
        MoveShild();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isCharging = false;
            ReleaseAttack();
        }


        if (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;
        }
    }

    private void MoveShild()
    {
        float horizontal = (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        float vertical = (Input.GetKey(KeyCode.S) ? -1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0);

        Vector3 shieldInput = new Vector3(horizontal, vertical, 0);

        // 입력x -> 마지막으로 바라본 방향 유지
        if (shieldInput == Vector3.zero)
        {
            shildDirection = status.viewDirection;
        }
        else
        {
            shildDirection = EightDirection.FromVector3(horizontal, vertical, 0);
        }

        // 위치 그대로
        ShildObject.transform.localPosition = Vector3.zero; //shildDirection / 1f; // 2f

        // 스프라이트 교체
        UpdateShieldSprite(shildDirection);
    }

    private void UpdateShieldSprite(Vector3 dir)
    {
        if (shieldSR == null) return;

        int sx = dir.x > 0.1f ? 1 : (dir.x < -0.1f ? -1 : 0);
        int sy = dir.y > 0.1f ? 1 : (dir.y < -0.1f ? -1 : 0);

        Sprite s = shieldDown;
        if (sx == 0 && sy > 0) s = shieldUp;
        else if (sx == 0 && sy < 0) s = shieldDown;
        else if (sx > 0 && sy == 0) s = shieldRight;
        else if (sx < 0 && sy == 0) s = shieldLeft;
        else if (sx > 0 && sy > 0) s = shieldUpRight;
        else if (sx < 0 && sy > 0) s = shieldUpLeft;
        else if (sx > 0 && sy < 0) s = shieldDownRight;
        else if (sx < 0 && sy < 0) s = shieldDownLeft;

        shieldSR.sprite = s;

        // 깔끔할지도 ??
        ShildObject.transform.localRotation = Quaternion.identity;
    }



    public void ReceiveAttack(IProjectile projectile)
    {
        StartCoroutine(CameraManager.Instance.WobbleEffect(projectile.MoveDirection, 0.1f * projectile.Damage));

        if (IsReflectable(projectile))
        {
            if (isCharging)
            {
                Charging(projectile);
                return;
            }
            else
            {
                Reflect(projectile);
                return;
            }
        }
        else
        {
            TakeDamage(projectile);
            return;
        }
    }

    private bool IsReflectable(IProjectile projectile)
    {
        if (projectile.MoveDirection == Vector3.zero) return false;

        Vector3 porjectileDirection = projectile.MoveDirection;
        float angle = Vector2.SignedAngle(-projectile.MoveDirection, shildDirection);
        bool isReflectable = angle >= -45f && angle <= 45f;

        return isReflectable;
    }

    private void Reflect(IProjectile projectile)
    {
        Vector3 porjectileDirection = projectile.MoveDirection;
        Vector3 R = 2f * Vector3.Dot(porjectileDirection, shildDirection.normalized) * shildDirection.normalized - porjectileDirection;

        projectile.Reflect(transform.position, -R, gameObject.tag);

    }

    private void TakeDamage(IProjectile projectile)
    {
        if (invincibleTimer > 0)
        {
            return;
        }

        status.CurrentHealth = status.CurrentHealth - projectile.Damage;
        invincibleTimer = invincibleTime;
    }

    private void Charging(IProjectile projectile)
    {
        if (isCharging)
        {
            status.CurrentChargingPower += projectile.Damage;
        }
    }

    private void ReleaseAttack()
    {
        if (!isCharging)
        {
            if (status.ChargedMax)
            {
                status.CurrentChargingPower = 0;

                ObjectPoolingManager.Instance.GetPrefab(ChargingAttackProjectilePrefab)
                    .GetComponent<IProjectile>().
                    Reflect(transform.position, shildDirection, gameObject.tag);
            }
        }

    }
}
