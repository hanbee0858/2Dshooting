using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Points")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;      // 비어 있으면 자동 수집/생성

    [Header("Rules")]
    public float spawnInterval = 2f;
    public int maxAlive = 10;

    [Header("Root (optional)")]
    public Transform enemiesRoot;        // 비워두면 Enemies_Container 자동 생성

    [Header("Options")]
    public bool spawnOnEnable = true;
    public int initialSpawnCount = 0;

    Transform EnsureRoot()
    {
        if (enemiesRoot == null)
        {
            var existed = GameObject.Find("Enemies_Container");
            enemiesRoot = existed ? existed.transform : new GameObject("Enemies_Container").transform;
        }
        return enemiesRoot;
    }

    void Awake()
    {
        // 스폰 포인트 자동 수집
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            var list = new System.Collections.Generic.List<Transform>();
            foreach (Transform c in transform) list.Add(c);

            if (list.Count == 0)
            {
                // 아무 자식도 없으면 자신의 위치에 AutoSpawnPoint 생성
                var p = new GameObject("AutoSpawnPoint").transform;
                p.SetParent(transform);
                p.localPosition = Vector3.zero;
                list.Add(p);
            }
            spawnPoints = list.ToArray();
        }
        EnsureRoot();
    }

    void OnEnable()
    {
        if (!spawnOnEnable) return;
        if (enemyPrefab == null) { Debug.LogError("[Spawner] enemyPrefab 미지정", this); return; }

        StopAllCoroutines();
        StartCoroutine(SpawnLoop());

        for (int i = 0; i < initialSpawnCount; i++) SpawnOne();
    }

    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);
        while (enabled && gameObject.activeInHierarchy)
        {
            if (Time.timeScale <= 0f) { yield return null; continue; }
            if (AliveCount() < maxAlive) SpawnOne();
            yield return wait;
        }
    }

    int AliveCount() => EnsureRoot().childCount;

    void SpawnOne()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

        int idx = Random.Range(0, spawnPoints.Length);
        Transform p = spawnPoints[idx];
        Instantiate(enemyPrefab, p.position, p.rotation, EnsureRoot());
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.red;
        foreach (var t in spawnPoints) if (t) Gizmos.DrawWireSphere(t.position, 0.25f);
    }
#endif
}
