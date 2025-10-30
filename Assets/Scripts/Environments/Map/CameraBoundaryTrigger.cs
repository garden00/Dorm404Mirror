using UnityEngine;

// �� ��ũ��Ʈ�� BoxCollider2D�� �ݵ�� �ʿ��մϴ�.
[RequireComponent(typeof(BoxCollider2D))]
public class CameraBoundaryTrigger : MonoBehaviour
{
    private BoxCollider2D mapCollider;

    void Awake()
    {
        mapCollider = GetComponent<BoxCollider2D>();
        mapCollider.isTrigger = true;
    }

    // �� ���� �� �÷��̾ �̹� �� �� �ȿ� ���� ��츦 ���
    private void Start()
    {
        if (CameraManager.Instance == null || CameraManager.Instance.target == null) return;

        // �÷��̾��� ���� ��ġ(target.position)�� �� �ݶ��̴� ��� �ȿ� �ִ��� Ȯ��
        if (mapCollider.bounds.Contains(CameraManager.Instance.target.position))
        {
            // �� ���� �� �÷��̾ �� �濡 �����Ƿ�, �� ���� ���� ��� ����
            CameraManager.Instance.UpdateCameraBounds(mapCollider.bounds);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �÷��̾ �� "��" Ʈ���� ������ ������ ��
        if (other.CompareTag("Player"))
        {
            // ī�޶� �Ŵ������� "���� �� ��踦 �����"��� �˸�
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.UpdateCameraBounds(mapCollider.bounds);
            }
        }
    }
}