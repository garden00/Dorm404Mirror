using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureLightning : MonoBehaviour, IProjectile
{
    [Header("Projectile Stats")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float maxLifetime = 5f;

    [Header("Visuals")]
    [SerializeField] private TrailRenderer trail; // 꼬리 효과 (있다면)

    private Vector3 moveDirection;
    private string ownerTag;
    private float lifeTimer;

    // --- IProjectile 구현 ---
    public int Damage => damage;
    public Vector3 MoveDirection => moveDirection;

    public void Fire(Vector3 _position, Vector3 _direction, string _ownerTag)
    {
        transform.position = _position;
        moveDirection = _direction.normalized;
        ownerTag = _ownerTag;
        lifeTimer = 0f;

        // 진행 방향으로 회전 (전기 화살 같은 느낌)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 트레일 초기화
        if (trail != null) trail.Clear();

        gameObject.SetActive(true);
    }

    public void Reflect(Vector3 _position, Vector3 _direction, string _ownerTag)
    {
        // 1페이즈 핵심 기믹: 반사
        moveDirection = _direction.normalized;
        ownerTag = _ownerTag; // 주인이 Player로 변경됨 -> 보스가 맞으면 아군 공격으로 인식

        // 반사되면 속도를 좀 더 빠르게 하여 타격감 상승
        speed *= 1.5f;

        // 방향 전환에 따른 회전 갱신
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    // ------------------------

    private void Update()
    {
        // 이동 로직
        transform.position += moveDirection * speed * Time.deltaTime;

        // 수명 체크
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= maxLifetime)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 발사한 주인(보통 Enemy)과는 충돌 무시
        //    단, 반사되어서 ownerTag가 "Player"가 되었다면 Enemy(보스)와 충돌 가능
        if (collision.CompareTag(ownerTag)) return;

        // 2. 다른 투사체와 충돌 무시
        if (collision.GetComponent<IProjectile>() != null) return;

        // 3. IDamageable 인터페이스를 가진 대상 타격
        //    (Player, Dracula, StreetLight 모두 IDamageable을 가지고 있음)
        if (collision.TryGetComponent<IDamageable>(out IDamageable target))
        {
            // 공격 정보 생성
            DamageInfo info = new DamageInfo(this, AttackType.Projectile, damage, moveDirection);

            // 데미지 전달
            // - Player가 맞으면: 데미지 입음
            // - StreetLight가 맞으면: ReceiveAttack에서 불이 켜짐 (2페이즈 기믹)
            // - Dracula가 맞으면: 
            //    -> 그냥 쏜 거면 데미지 입음 (혹은 무시)
            //    -> 반사된 거면(ownerTag == Player) 폭주 게이지 상승 (1페이즈 기믹)
            target.ReceiveAttack(info);

            Despawn();
        }
        // 4. 벽에 닿으면 소멸
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        // 속도 등 변수 초기화가 필요하다면 여기서 수행
        speed = 7f;

        // 풀링 반환
        ObjectPoolingManager.Instance.Return(gameObject);
    }
}
