using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

// 할꺼 : 카메라 관련

public class PlayerCombat : MonoBehaviour, IDamageable, IEffectable
{
    private PlayerStatusData status;
    private ShieldController shield;

    [Header("Settings")]
    [SerializeField] private float invincibleTime = 1.0f;
    [SerializeField] private GameObject chargingAttackProjectilePrefab;
    [SerializeField] private float WobbleEffectPower = 0.05f;

    private float invincibleTimer;
    private bool isCharging;

    public event Action OnReflectEvent; // 애니메이션 연동용

    public void Initialize(PlayerStatusData playerStatus)
    {
        status = playerStatus;
        shield = GetComponent<ShieldController>();
    }

    private void Update()
    {
        HandleCooldowns();
        HandleInput();
    }

    private void HandleCooldowns()
    {
        if (invincibleTimer > 0) invincibleTimer -= Time.deltaTime;
    }

    private void HandleInput()
    {
        if (!status.IsActionable && !isCharging) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            status.SetState(PlayerState.Attacking); // 상태 변경 (선택 사항)
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isCharging = false;
            TryReleaseChargedAttack();
            if (status.CurrentState == PlayerState.Attacking)
                status.SetState(PlayerState.Idle);
        }
    }

    public void ReceiveAttack(DamageInfo info)
    {

        // info.type이 Projectile이고, source가 IProjectile로 변환 가능하다면 'proj'에 담음
        if (info.type == AttackType.Projectile && info.source is IProjectile proj)
        {
            // 카메라 흔들림
            StartCoroutine(CameraManager.Instance.WobbleEffect(info.direction, WobbleEffectPower * info.damage * 0.5f));

            // 투사체 반사 가능 여부 체크
            if (IsReflectable(proj))
            {
                if (isCharging)
                {
                    AbsorbProjectile(proj); // 흡수
                }
                else
                {
                    ReflectProjectile(proj); // 반사
                }
                return; // 반사/흡수했으면 데미지 안 입고 종료
            }
        }

        // 3. 그 외 (근접 공격이거나, 반사 불가능한 투사체인 경우)
        TakeDamage(info);
    }

    private bool IsReflectable(IProjectile projectile)
    {
        if (projectile.MoveDirection == Vector3.zero || shield == null) return false;

        float angle = Vector2.SignedAngle(-projectile.MoveDirection, shield.Direction);
        return Mathf.Abs(angle) <= 45f;
    }

    private void ReflectProjectile(IProjectile projectile)
    {
        Vector3 reflectVec = Vector3.Reflect(projectile.MoveDirection, shield.Direction.normalized);
        projectile.Reflect(transform.position, reflectVec, gameObject.tag);

        OnReflectEvent?.Invoke();
    }

    private void AbsorbProjectile(IProjectile projectile)
    {
        status.CurrentChargingPower += projectile.Damage;
        // 투사체 제거 로직은 보통 projectile.Reflect 혹은 별도 Destroy 호출 필요
        // 여기서는 흡수했으므로 투사체가 사라지는 로직이 필요함 (가정)
        //projectile.Deactivate();
    }

    private void TakeDamage(DamageInfo damageInfo)
    {
        switch (damageInfo.projectileEffect)
        {
            case ProjectileEffect.Normal:
                if (invincibleTimer > 0) return;

                // 카메라 흔들림
                StartCoroutine(CameraManager.Instance.WobbleEffect(damageInfo.direction, WobbleEffectPower * damageInfo.damage));

                invincibleTimer = invincibleTime;
                status.CurrentHealth -= damageInfo.damage;
                // 피격 위치 전달 (넉백 계산용)
                status.RaiseOnHitEvent(damageInfo.direction == EightDirection.None ? -status.ViewDirection : damageInfo.direction);
                break;


            case ProjectileEffect.Poison:
                if (invincibleTimer > 0) return;

                // 카메라 흔들림
                StartCoroutine(CameraManager.Instance.WobbleEffect(damageInfo.direction, WobbleEffectPower * damageInfo.damage));

                invincibleTimer = invincibleTime;
                ApplyPoison(5, 5f); // 5초간 틱당 5데미지
                status.CurrentHealth -= damageInfo.damage;
                break;

            case ProjectileEffect.Slow:
                ApplySpeedChange(0.5f, 3f); // 3초간 속도 50%
                status.CurrentHealth -= damageInfo.damage;
                break;

            case ProjectileEffect.SpeedUp:
                ApplySpeedChange(1.5f, 3f); // 3초간 속도 150%
                status.CurrentHealth -= damageInfo.damage;
                break;
            case ProjectileEffect.Heal:
                ApplyHeal(30); // 30 회복
                status.CurrentHealth -= damageInfo.damage;
                break;
        }
    }


    // 개구리 같은 근접 공격용
    public void ReceiveMeleeDamage(int damage, Vector3 attackDir)
    {
        if (invincibleTimer > 0) return;

        status.CurrentHealth -= damage;
        invincibleTimer = invincibleTime;

        // 넉백 방향 정수
        Vector3 kbDir = attackDir;

        if (kbDir.sqrMagnitude < 0.001f)
            kbDir = -status.ViewDirection;

        if (Mathf.Abs(kbDir.x) > Mathf.Abs(kbDir.y))
            kbDir = new Vector3(Mathf.Sign(kbDir.x), 0, 0);   // 좌우 고정
        else
            kbDir = new Vector3(0, Mathf.Sign(kbDir.y), 0);   // 상하 고정

        // 넉백 이벤트 전달
        status.RaiseOnHitEvent(kbDir);
    }


    private void TryReleaseChargedAttack()
    {
        if (status.IsMaxCharged)
        {
            status.CurrentChargingPower = 0;

            var projectileObj = ObjectPoolingManager.Instance?.GetPrefab(chargingAttackProjectilePrefab);
            if (projectileObj != null)
            {
                var proj = projectileObj.GetComponent<IProjectile>();
                proj?.Reflect(transform.position, shield.Direction, gameObject.tag);
            }
        }
    }

    // === IEffectable ===

    public void ApplySpeedChange(float multiplier, float duration)
    {
        StartCoroutine(SpeedChangeRoutine(multiplier, duration));
    }

    private IEnumerator SpeedChangeRoutine(float multiplier, float duration)
    {
        status.speedMultiplier = multiplier;
        var renderer = GetComponent<SpriteRenderer>();
        if(multiplier > 1f)
            renderer.color = Color.blue;
        else
            renderer.color = Color.green;

            yield return new WaitForSeconds(duration);

        status.speedMultiplier = 1f; // 원상 복구
        renderer.color = Color.white;
    }

    public void ApplyPoison(int damagePerTick, float duration)
    {
        StartCoroutine(PoisonRoutine(damagePerTick, duration));
    }

    private IEnumerator PoisonRoutine(int damage, float duration)
    {
        var renderer = GetComponent<SpriteRenderer>();
        renderer.color = Color.magenta; // 독 효과

        float timer = 0f;
        while (timer < duration)
        {
            // 독 데미지는 무적시간 무시
            status.CurrentHealth -= damage;
            // 독 데미지 연출 (깜빡임 등)

            yield return new WaitForSeconds(1f);
            timer += 1f;
        }

        renderer.color = Color.white; // 독 효과
    }

    public void ApplyHeal(int amount)
    {
        status.Healing(amount);
    }
}