using UnityEngine;

// 이 스크립트는 BoxCollider2D가 반드시 필요합니다.
[RequireComponent(typeof(BoxCollider2D))]
public class CameraBoundaryTrigger : MonoBehaviour
{
    private BoxCollider2D mapCollider;

    void Awake()
    {
        mapCollider = GetComponent<BoxCollider2D>();
        mapCollider.isTrigger = true;
    }

    // 씬 시작 시 플레이어가 이미 이 방 안에 있을 경우를 대비
    private void Start()
    {
        if (CameraManager.Instance == null || CameraManager.Instance.target == null) return;

        // 플레이어의 현재 위치(target.position)가 이 콜라이더 경계 안에 있는지 확인
        if (mapCollider.bounds.Contains(CameraManager.Instance.target.position))
        {
            // 씬 시작 시 플레이어가 이 방에 있으므로, 이 방의 경계로 즉시 설정
            CameraManager.Instance.UpdateCameraBounds(mapCollider.bounds);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어가 이 "방" 트리거 안으로 들어왔을 때
        if (other.CompareTag("Player"))
        {
            // 카메라 매니저에게 "이제 이 경계를 사용해"라고 알림
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.UpdateCameraBounds(mapCollider.bounds);
            }
        }
    }
}