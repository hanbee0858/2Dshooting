using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 일정 시간마다 적을 여러 지점에 랜덤 스폰하는 스크립트.
/// 스폰 포인트가 없으면 자동 생성하며,
/// Enemy Prefab이 비어 있으면 Resources 폴더에서 자동으로 로드합니다.
/// </summary>
public sealed class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Points")]
    [SerializeField] private GameObject enemyPrefab;           // EnemyWander 포함 프리팹
    [SerializeField] private List<Transform> spawnPoints = new();

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1.5f;       // 생성 간격(초)
    [SerializeField] private int maxAlive = 8;                  // 동시에 존재 가능한 최대 적 수
    [SerializeField] private int initialSpawnCount = 3;         // 시작 시 생성할 적 수
    [SerializeField] private float jitterRadius = 0.5f;         // 스폰 포인트 주변 랜덤 오프셋 반경

    [Header("Parent Group (Optional)")]
    [SerializeField] private Transform enemiesRoot;             // 생성된 적들을 모을 부모 오브젝트

    [Header("Fallback Resource (Optional)")]
    [SerializeField] private string prefabResourcePath = "EnemyFinal"; // Assets/Resources/EnemyFinal.prefab

    private Coroutine _spawnLoop;

    private void Awake()
    {
        Debug.Log($"[Spawner/Awake] obj={name}, prefab={enemyPrefab?.name ?? "NULL"}", this);

        // 🔹 스폰 포인트 자동 수집 또는 생성
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            CollectSpawnPoints();
        }

        CreateDefaultPointsIfEmpty();

        // 🔹 Enemy 프리팹 자동 보강
        if (enemyPrefab == null && !string.IsNullOrWhiteSpace(prefabResourcePath))
        {
            enemyPrefab = Resources.Load<GameObject>(prefabResourcePath);
            if (enemyPrefab != null)
            {
                Debug.Log($"[Spawner] Resources로 prefab 보강 성공: {enemyPrefab.name}", this);
            }
        }
    }

    private void OnEnable()
    {
        Debug.Log($"[Spawner/OnEnable] obj={name}, prefab={(enemyPrefab ? enemyPrefab.name : "NULL")}", this);
        _spawnLoop ??= StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (_spawnLoop != null)
        {
            StopCoroutine(_spawnLoop);
            _spawnLoop = null;
        }
    }

    /// <summary>
    /// 스폰 루프 코루틴.
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        yield return null;
        SpawnInitial();

        var wait = new WaitForSeconds(spawnInterval);
        while (enabled)
        {
            TrySpawnOne();
            yield return wait;
        }
    }

    /// <summary>
    /// 시작 시 초기 스폰.
    /// </summary>
    private void SpawnInitial()
    {
        for (int i = 0; i < initialSpawnCount; i++)
        {
            TrySpawnOne();
        }
    }

    /// <summary>
    /// 단일 적 스폰 시도.
    /// </summary>
    private void TrySpawnOne()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"[Spawner] enemyPrefab 미지정 | obj={name}", this);
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError($"[Spawner] spawnPoints 미지정 | obj={name}", this);
            return;
        }

        // 현재 존재하는 적 수 확인 (EnemyWander 기준)
        int alive = FindObjectsByType<EnemyWander>(FindObjectsSortMode.None).Length;
        if (alive >= maxAlive)
        {
            return;
        }

        // 랜덤 스폰 포인트 + 주변 오프셋
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
        Vector2 offset = Random.insideUnitCircle * jitterRadius;
        Vector3 spawnPos = point.position + new Vector3(offset.x, offset.y, 0f);

        // 적 생성
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        if (enemiesRoot != null)
        {
            enemy.transform.SetParent(enemiesRoot, true);
        }

        Debug.Log($"[Spawner] Spawned {enemy.name} at {spawnPos}");
    }

    /// <summary>
    /// 자식 오브젝트 중 이름에 "point"가 포함된 Transform 자동 수집.
    /// </summary>
    [ContextMenu("Collect Spawn Points From Children")]
    private void CollectSpawnPoints()
    {
        spawnPoints ??= new List<Transform>();
        spawnPoints.Clear();

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child == transform) continue;
            if (child.name.ToLower().Contains("point"))
            {
                spawnPoints.Add(child);
            }
        }

        Debug.Log($"[Spawner] 수집된 포인트: {spawnPoints.Count}개 | obj={name}", this);
    }

    /// <summary>
    /// 스폰 포인트가 전혀 없을 경우, 기본 포인트 3개 자동 생성.
    /// </summary>
    private void CreateDefaultPointsIfEmpty()
    {
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            return;
        }

        spawnPoints = new List<Transform>();
        Vector3 basePos = transform.position;

        Vector3[] localPositions =
        {
            new(-3f, 0f, 0f),
            new(0f, 3f, 0f),
            new(3f, 0f, 0f)
        };

        foreach (Vector3 pos in localPositions)
        {
            GameObject point = new GameObject($"SpawnPoint_{spawnPoints.Count + 1}");
            point.transform.SetParent(transform);
            point.transform.position = basePos + pos;
            spawnPoints.Add(point.transform);
        }

        Debug.Log($"[Spawner] 기본 포인트 자동 생성 완료 ({spawnPoints.Count}개).", this);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 값이 비어있으면 자동 보강 (즉시 반영).
    /// </summary>
    private void OnValidate()
    {
        if (enemyPrefab == null && !string.IsNullOrWhiteSpace(prefabResourcePath))
        {
            var loaded = Resources.Load<GameObject>(prefabResourcePath);
            if (loaded != null)
            {
                enemyPrefab = loaded;
            }
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            CollectSpawnPoints();
        }
    }
#endif
}
