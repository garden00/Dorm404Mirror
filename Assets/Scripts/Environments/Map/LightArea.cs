using UnityEngine;

public class LightArea : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        // 플레이어가 범위 안에 있으면 충전
        if (other.CompareTag("Player"))
        {
            // PlayerManager가 있는지 확인 후 충전
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.Charge(Time.deltaTime);
                //Debug.Log("플레이어 충전 중...");
            }
        }
    }
}