using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPrjtle : MonoBehaviour, IProjectile
{
    [Header("Bomb Stats")]
    [SerializeField] private float exploTime = 2f;
    [SerializeField] private float lifeTimeLimit = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int damage = 10;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private LayerMask damageLayer;        // 데미지를 줄 대상 (Player, Enemy)
    [SerializeField] private LayerMask obstacleLayer;      // 벽이나 바닥 등 터져야 하는 장애물 레이어 (새로 추가됨)
    [SerializeField] private GameObject explosionEffectPrefab;

    private float _currentExploTimer = 0f;
    private float _currentLifeTimer = 0f;
    private Vector3 _moveDirection;
    private string _ownerTag;
    private bool _isExploded = false;

    public int Damage => damage;
    public Vector3 MoveDirection => _moveDirection;

    void Update()
    {
        HandleLifeCycle();
        Move();
    }

    private void Move()
    {
        if (_isExploded) return;
        transform.position += _moveDirection * moveSpeed * Time.deltaTime;
    }

    // [수정됨] 충돌 로직 개선
    void OnTriggerEnter2D(Collider2D other)
    {
        if (_isExploded) return;

        // 1. 안전 장치: 생성 직후 0.1초 동안은 절대 터지지 않음 (발사체 끼임 방지)
        if (_currentLifeTimer < 0.1f) return;

        // 2. 주인 확인
        if (other.CompareTag(_ownerTag)) return;

        // 3. 레이어 필터링: '데미지 줄 대상'이거나 '장애물'인 경우에만 폭발
        // (트리거 존이나 아이템 등에 닿아서 터지는 것 방지)
        bool isTarget = ((1 << other.gameObject.layer) & damageLayer) != 0;
        bool isObstacle = ((1 << other.gameObject.layer) & obstacleLayer) != 0;

        if (isTarget || isObstacle)
        {
            StartCoroutine(Explo());
        }
        else
        {
            // 디버깅용: 무엇 때문에 안 터졌는지 확인 (필요 없으면 삭제)
            // Debug.Log($"무시된 충돌 대상: {other.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
        }
    }

    public void Fire(Vector3 position, Vector3 direction, string ownerTag)
    {
        _currentLifeTimer = 0;
        _currentExploTimer = 0;
        _isExploded = false;

        this._ownerTag = ownerTag;
        this._moveDirection = direction.normalized;
        transform.position = position + _moveDirection * 0.5f;

        // 중요: 재사용 시 이펙트가 켜져 있을 수 있으므로 끄기
        if (explosionEffectPrefab != null)
            explosionEffectPrefab.SetActive(false);
    }

    public void Reflect(Vector3 position, Vector3 direction, string ownerTag)
    {
        this._ownerTag = ownerTag;
        this._moveDirection = direction.normalized;
        transform.position = position;
        _currentExploTimer = 0;

        float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void HandleLifeCycle()
    {
        if (_isExploded) return;

        _currentExploTimer += Time.deltaTime;
        // 타이머 다 되면 폭발
        if (_currentExploTimer > exploTime)
        {
            StartCoroutine(Explo());
            return;
        }

        _currentLifeTimer += Time.deltaTime;
        // 수명 다 되면(불발) 조용히 사라짐
        if (_currentLifeTimer > lifeTimeLimit)
        {
            Die();
        }
    }

    private IEnumerator Explo()
    {
        if (_isExploded) yield break;
        _isExploded = true;

        // 1. 범위 데미지 처리
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(_ownerTag)) continue;

            if (hitCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                Vector3 knockbackDir = (hitCollider.transform.position - transform.position).normalized;
                DamageInfo info = new DamageInfo(this, AttackType.Melee, damage);
                damageable.ReceiveAttack(info);
            }
        }

        // 2. 이펙트 재생
        if (explosionEffectPrefab != null)
        {
            // 부모-자식 관계라면 부모(폭탄)가 사라질 때 이펙트도 같이 사라지는 문제가 있습니다.
            // 해결책: 이펙트를 부모에서 분리(Detach)하거나, 별도로 생성해야 합니다.
            // 여기서는 일단 기존 코드대로 SetActive를 사용합니다.
            explosionEffectPrefab.SetActive(true);

            // 이펙트가 보이도록 잠깐 대기
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // 이펙트 없으면 잠깐만 대기
            yield return new WaitForSeconds(0.1f);
        }

        // 3. 제거
        Die();
    }

    private void Die()
    {
        // 다음 사용을 위해 이펙트 끄기 (Object Pooling 고려)
        if (explosionEffectPrefab != null)
            explosionEffectPrefab.SetActive(false);

        ObjectPoolingManager.Instance.Return(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}