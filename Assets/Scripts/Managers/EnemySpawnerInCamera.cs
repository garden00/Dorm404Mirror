using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawnerInCamera : MonoBehaviour
{

    [Header("Ground 타일맵")]
    [SerializeField] private Tilemap groundTilemap;

    [Header("기본 설정")]
    public Transform player;                  // 플레이어
    public GameObject[] enemyPrefabs;         // 이 스테이지에서 뽑을 몬스터들

    [Header("스폰 주기 & 최대 수")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f; 
    [SerializeField, Min(0)] private int maxAliveEnemies = 10;    

    [Header("플레이어와 거리 조건")]
    [SerializeField, Min(0f)] private float minDistanceFromPlayer = 3f;
    [SerializeField] private float maxDistanceFromPlayer = 999f;


    [Header("장애물 체크")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Range(0.1f, 1f)] private float obstacleCheckRadius = 0.3f;

    private float timer = 0f;
    private Camera mainCam;

    // 간단히 살아있는 몬스터 추적용
    private List<GameObject> aliveEnemies = new List<GameObject>();

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("[EnemySpawnerInCamera] Main Camera를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (player == null || mainCam == null) return;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // 죽은 몬스터들 리스트에서 제거
        CleanupDeadEnemies();

        // 최대 수를 넘었으면 스폰 안 함
        if (aliveEnemies.Count >= maxAliveEnemies) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnEnemy();
        }
    }

    private void CleanupDeadEnemies()
    {
        // null 이 된(파괴된) 오브젝트는 리스트에서 제거
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null || !aliveEnemies[i].activeInHierarchy)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    private void TrySpawnEnemy()
    {
        const int maxTryCount = 20;

        for (int i = 0; i < maxTryCount; i++)
        {
            // --- 1. 카메라 밖 랜덤 위치 만들기 ---
            float vx, vy;

            // 랜덤으로 바깥 방향 선택
            int side = Random.Range(0, 4);

            switch (side)
            {
                case 0: // 왼쪽 바깥
                    vx = Random.Range(-0.3f, -0.05f);
                    vy = Random.Range(0f, 1f);
                    break;

                case 1: // 오른쪽 바깥
                    vx = Random.Range(1.05f, 1.3f);
                    vy = Random.Range(0f, 1f);
                    break;

                case 2: // 위쪽 바깥
                    vx = Random.Range(0f, 1f);
                    vy = Random.Range(1.05f, 1.3f);
                    break;

                default: // 아래쪽 바깥
                    vx = Random.Range(0f, 1f);
                    vy = Random.Range(-0.3f, -0.05f);
                    break;
            }

            Vector3 viewportPos = new Vector3(vx, vy, 0f);
            Vector3 worldPos = mainCam.ViewportToWorldPoint(viewportPos);
            worldPos.z = 0f;

            // 2) ★ Ground 타일이 있는 칸인지 확인 + 칸 중앙으로 스냅
            if (groundTilemap != null)
            {
                Vector3Int cellPos = groundTilemap.WorldToCell(worldPos);

                // 이 칸에 Ground 타일이 없으면 다시 시도
                if (!groundTilemap.HasTile(cellPos))
                    continue;

                // 그 칸의 "정중앙" 좌표로 스폰 위치 고정
                worldPos = groundTilemap.GetCellCenterWorld(cellPos);
            }

            // 3) 플레이어와 거리 조건
            float distToPlayer = Vector3.Distance(worldPos, player.position);
            if (distToPlayer < minDistanceFromPlayer) continue;
            if (distToPlayer > maxDistanceFromPlayer) continue;

            // 4) 장애물(벽) 체크 (있으면 피하기)
            if (obstacleMask != 0)
            {
                Collider2D hit = Physics2D.OverlapCircle(worldPos, obstacleCheckRadius, obstacleMask);
                if (hit != null)
                {
                    continue; // 벽/기둥이면 다른 위치 다시 시도
                }
            }

            // 5) 여기까지 통과했으면 실제 스폰
            SpawnEnemy(worldPos);
            break;
        }
    }

    private void SpawnEnemy(Vector3 position)
    {
        // 스테이지 몬스터 중 하나 랜덤 선택
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        if (prefab == null) return;

        GameObject enemyObj = null;

        // 풀 매니저가 있으면 풀 사용, 아니면 Instantiate
        if (ObjectPoolingManager.Instance != null)
        {
            enemyObj = ObjectPoolingManager.Instance.GetPrefab(prefab);
            if (enemyObj == null) return;
            enemyObj.transform.position = position;
        }
        else
        {
            enemyObj = Instantiate(prefab, position, Quaternion.identity);
        }

        aliveEnemies.Add(enemyObj);
    }
}
