using UnityEngine;
using System.Collections;

public class EnemySpawer : MonoBehaviour
{
    [Header("Prefab & Points")]
    public GameObject enemyPrefab;              // 생성할 적 프리팹 (필수)
    public Transform[] spawnPoints;             // 비어있으면 자동 수집/자동 생성

    [Header("Spawn Rules")]
    public float spawnInterval = 2f;            // 초
    public int maxAlive = 10;                 // 동시에 존재 가능한 최대치
    public bool spawnOnEnable = true;          // 활성화 시 루프 자동 시작
    public int initialSpawnCount = 1;         // 시작 직후 즉시 스폰 수(0이면 생략)

    [Header("Diagnostics")]
    public bool verbose = true;               // 로그 표시
    public string enemyTag = "Enemy";           // 카운팅 태그(선택)

    // ──────────────────────────────────────────────────────────────────
    void Awake()
    {
        EnsureSpawnPoints();
    }

    void OnEnable()
    {
        if (!spawnOnEnable) return;

        if (!CheckReady()) return;              // 준비 안 되면 로그 후 종료
        StopAllCoroutines();
        StartCoroutine(SpawnLoop());

        // 시작하자마자 확인용으로 즉시 N마리 스폰(선택)
        if (initialSpawnCount > 0)
        {
            for (int i = 0; i < initialSpawnCount; i++) SpawnOne();
        }
    }

    // 준비 상태 점검
    bool CheckReady()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[Spawner] enemyPrefab이 비었습니다! 프리팹을 연결하세요.", this);
            return false;
        }
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[Spawner] spawnPoints가 없습니다! (자동 생성 실패) 수동으로 추가하세요.", this);
            return false;
        }
        return true;
    }

    // 스폰 포인트 자동 수집/자동 생성
    void EnsureSpawnPoints()
    {
        // 1) 자식들 자동 수집
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            var list = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in transform) list.Add(child);
            spawnPoints = list.ToArray();

            if (verbose) Debug.Log($"[Spawner] 자동 수집: {spawnPoints.Length}개", this);
        }

        // 2) 그래도 없으면 자동 생성(현재 위치 1개)
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            var p = new GameObject("AutoSpawnPoint").transform;
            p.SetParent(transform);
            p.localPosition = Vector3.zero;
            spawnPoints = new Transform[] { p };

            if (verbose) Debug.Log("[Spawner] 스폰포인트가 없어 AutoSpawnPoint를 생성했습니다.", this);
        }
    }

    IEnumerator SpawnLoop()
    {
        if (verbose) Debug.Log("[Spawner] SpawnLoop 시작", this);

        var wait = new WaitForSeconds(spawnInterval);
        while (enabled && gameObject.activeInHierarchy)
        {
            if (Time.timeScale <= 0f) { yield return null; continue; }

            int alive = CountAlive();
            if (alive < maxAlive)
            {
                SpawnOne();
            }
            else if (verbose)
            {
                Debug.Log($"[Spawner] 상한 도달: {alive}/{maxAlive}", this);
            }

            yield return wait;
        }
    }

    int CountAlive()
    {
        if (!string.IsNullOrEmpty(enemyTag))
        {
            var arr = GameObject.FindGameObjectsWithTag(enemyTag);
            return arr?.Length ?? 0;
        }

#if UNITY_2023_1_OR_NEWER
        var enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
#else
        var enemies = Object.FindObjectsOfType<Enemy>(false);
#endif
        return enemies?.Length ?? 0;
    }

    void SpawnOne()
    {
        if (enemyPrefab == null) { Debug.LogError("[Spawner] enemyPrefab NULL", this); return; }
        if (spawnPoints == null || spawnPoints.Length == 0) { Debug.LogError("[Spawner] spawnPoints 없음", this); return; }

        int idx = Random.Range(0, spawnPoints.Length);
        Transform p = spawnPoints[idx];
        GameObject go = Instantiate(enemyPrefab, p.position, p.rotation);

        if (!string.IsNullOrEmpty(enemyTag))
        {
            try { go.tag = enemyTag; } catch { /* 태그 미등록 시 무시 */ }
        }

        if (verbose) Debug.Log($"[Spawner] Spawn @ {p.name} (현재 {CountAlive()}/{maxAlive})", this);
    }

    // 런타임 즉시 강제 스폰 테스트 버튼 (인스펙터에서 우클릭 메뉴)
    [ContextMenu("TEST: Spawn One Now")]
    void TestSpawnNow()
    {
        EnsureSpawnPoints();
        if (!CheckReady()) return;
        SpawnOne();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (spawnInterval < 0.05f) spawnInterval = 0.05f;
        if (maxAlive < 1) maxAlive = 1;
    }

    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        foreach (var t in spawnPoints)
        {
            if (t == null) continue;
            Gizmos.DrawWireSphere(t.position, 0.25f);
        }
    }
#endif
}