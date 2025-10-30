using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitBeam : MonoBehaviour, IProjectile
{
    // --- IProjectile 인터페이스 구현 ---

    [SerializeField]
    private int damage = 10;
    public int Damage => damage;

    private EightDirection laserDirection;
    public EightDirection MoveDirection => laserDirection;

    private string ownerTag;

    // --- Lazer 고유 속성 ---

    // LineRenderer 대신, 빔의 시각 효과를 담당할 자식 오브젝트의 Transform
    [SerializeField]
    private Transform laserBeamVisual;
    private SpriteRenderer beamRenderer;

    [SerializeField]
    private float maxDistance = 100f; // 레이저 최대 사거리

    [SerializeField]
    private float laserDuration = 0.2f; // 레이저가 보였다 사라지는 시간

    [SerializeField]
    private LayerMask hitMask; // 레이저가 충돌할 2D 레이어 마스크

    private void Awake()
    {
        if (laserBeamVisual != null)
        {
            beamRenderer = laserBeamVisual.GetComponent<SpriteRenderer>();
            laserBeamVisual.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Lazer: 'Laser Beam Visual'이 할당되지 않았습니다!");
        }
    }

    public void Fire(Vector3 _position, EightDirection _direction, string _ownerTag)
    {
        transform.position = _position;
        laserDirection = _direction;
        ownerTag = _ownerTag;

        Vector2 rayOrigin = _position;
        Vector2 rayDirection = laserDirection;
        Vector3 endPoint = _position + laserDirection.VectorGrid * maxDistance;


        int ownerLayerIndex = LayerMask.NameToLayer(_ownerTag);
        int layerMaskToExcludeOwner = ~(1 << ownerLayerIndex);
        int finalMask = hitMask & layerMaskToExcludeOwner;

        RaycastHit2D hitInfo = Physics2D.Raycast(rayOrigin, rayDirection, maxDistance, finalMask);

        if (hitInfo.collider != null)
        {
            endPoint = new Vector3(hitInfo.point.x, hitInfo.point.y, _position.z);

            if (hitInfo.collider.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.ReceiveAttack(this);
            }
        }
        else
        {
            endPoint = _position + _direction.VectorGrid * maxDistance;
        }

        StartCoroutine(ShowLaserEffect(_position, endPoint));
    }

    public void Reflect(Vector3 _position, EightDirection _direction, string _ownerTag)
    {
        GameObject myOriginalPrefab = ObjectPoolingManager.Instance.GetOriginalPrefab(gameObject);

        if (myOriginalPrefab == null)
        {
            Debug.LogError($"[Reflect] {gameObject.name}의 원본 프리팹을 ObjectPoolingManager에서 찾을 수 없습니다.", gameObject);
            return;
        }

        ObjectPoolingManager.Instance.GetPrefab(myOriginalPrefab)
            .GetComponent<IProjectile>().
            Fire(_position, _direction, _ownerTag);

        ObjectPoolingManager.Instance.GetPrefab(myOriginalPrefab)
            .GetComponent<IProjectile>().
            Fire(_position, _direction - 1, _ownerTag);

        ObjectPoolingManager.Instance.GetPrefab(myOriginalPrefab)
            .GetComponent<IProjectile>().
            Fire(_position, _direction + 1, _ownerTag);
    }

    /// <summary>
    /// 레이저(스프라이트)를 잠시 켰다가 끄는 코루틴 (수정)
    /// </summary>
    /// <summary>
    /// 레이저(스프라이트)를 페이드 인/아웃하며 켜고 끄는 코루틴
    /// </summary>
    private IEnumerator ShowLaserEffect(Vector3 startPoint, Vector3 endPoint)
    {
        if (beamRenderer == null)
        {
            yield break;
        }

        Transform beam = laserBeamVisual;
        Vector3 direction = (endPoint - startPoint);
        float distance = direction.magnitude;

        beam.localScale = new Vector3(distance, beam.localScale.y, 1f);
        beam.right = direction.normalized;
        beam.position = startPoint + (direction.normalized * (distance / 2));

        beam.gameObject.SetActive(true);

        Color beamColor = beamRenderer.color;
        float elapsedTime = 0;

        while (elapsedTime < laserDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / laserDuration;
            beamColor.a = Mathf.Clamp01(alpha);
            beamRenderer.color = beamColor;
            yield return null;
        }

        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;
            float alpha = elapsedTime / laserDuration;
            beamColor.a = Mathf.Clamp01(alpha);
            beamRenderer.color = beamColor;
            yield return null;
        }

        ObjectPoolingManager.Instance.Return(gameObject);
        beam.gameObject.SetActive(false);
    }
}
