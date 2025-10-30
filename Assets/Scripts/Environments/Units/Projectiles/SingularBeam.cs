using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingularBeam : MonoBehaviour, IProjectile
{
    // --- IProjectile �������̽� ���� ---

    [SerializeField]
    private int damage = 10;
    public int Damage => damage;

    private EightDirection laserDirection;
    public EightDirection MoveDirection => laserDirection;

    private string ownerTag;

    // --- Lazer ���� �Ӽ� ---

    // LineRenderer ���, ���� �ð� ȿ���� ����� �ڽ� ������Ʈ�� Transform
    [SerializeField]
    private Transform laserBeamVisual;
    private SpriteRenderer beamRenderer;

    [SerializeField]
    private float maxDistance = 100f; // ������ �ִ� ��Ÿ�

    [SerializeField]
    private float laserDuration = 0.2f; // �������� ������ ������� �ð�

    [SerializeField]
    private LayerMask hitMask; // �������� �浹�� 2D ���̾� ����ũ

    private void Awake()
    {
        if (laserBeamVisual != null)
        {
            beamRenderer = laserBeamVisual.GetComponent<SpriteRenderer>();
            laserBeamVisual.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Lazer: 'Laser Beam Visual'�� �Ҵ���� �ʾҽ��ϴ�!");
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
            Debug.LogError($"[Reflect] {gameObject.name}�� ���� �������� ObjectPoolingManager���� ã�� �� �����ϴ�.", gameObject);
            return;
        }

        ObjectPoolingManager.Instance.GetPrefab(myOriginalPrefab)
            .GetComponent<IProjectile>().
            Fire(_position, _direction, _ownerTag);
    }

    /// <summary>
    /// ������(��������Ʈ)�� ��� �״ٰ� ���� �ڷ�ƾ (����)
    /// </summary>
    /// <summary>
    /// ������(��������Ʈ)�� ���̵� ��/�ƿ��ϸ� �Ѱ� ���� �ڷ�ƾ
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
