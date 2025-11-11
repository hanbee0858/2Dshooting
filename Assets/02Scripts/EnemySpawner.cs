using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Points")]
    public GameObject enemyPrefab;                 // Project/Resources/EnemyFinal (파란 큐브)
    public string resourcePath = "EnemyFinal";     // Resources/EnemyFinal
    public List<Transform> spawnPoints = new();

    [Header("Rules")]
    public float spawnInterval = 1.5f;
    public int maxAlive = 8;
    public int initialSpawnCount = 4;            // 시작부터 여러 기

    [Header("Root (optional)")]
    public Transform enemiesRoot;

    [Header("Options")]
    public bool spawnOnEnable = true;
    public bool verbose = true;

    Coroutine loop;

    void OnEnable()
    {
        // 프리팹 확보
        if (!enemyPrefab && !string.IsNullOrEmpty(resourcePath))
            enemyPrefab = Resources.Load<GameObject>(resourcePath);

        if (!enemyPrefab) { Debug.LogError("[Spawner] enemyPrefab 미지정(또는 Resources/" + resourcePath + " 없음)", this); return; }

        // 스폰포인트 확보
        if (spawnPoints.Count == 0)
            foreach (Transform t in transform) if (t) spawnPoints.Add(t);
        if (spawnPoints.Count == 0) AutoCreatePoints();

        // 루트 확보
        if (!enemiesRoot)
        {
            var root = GameObject.Find("Enemies_Container");
            if (!root) root = new GameObject("Enemies_Container");
            enemiesRoot = root.transform;
        }

        // 시작 강제 스폰
        ForceSpawn(initialSpawnCount);

        // 루프 시작
        if (spawnOnEnable)
        {
            if (loop != null) StopCoroutine(loop);
            loop = StartCoroutine(SpawnLoop());
        }

        if (verbose) Debug.Log($"[Spawner] Ready points={spawnPoints.Count}, maxAlive={maxAlive}, interval={spawnInterval}", this);
    }

    void OnDisable() { if (loop != null) StopCoroutine(loop); }

    IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);
        while (true) { TrySpawnOne(); yield return wait; }
    }

    void TrySpawnOne()
    {
        int alive = CountAliveInScene();
        if (alive >= maxAlive) { if (verbose) Debug.Log($"[Spawner] alive={alive}/{maxAlive} 대기", this); return; }

        var p = spawnPoints[Random.Range(0, spawnPoints.Count)];
        SpawnOneAt(p.position);
    }

    void ForceSpawn(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var p = spawnPoints[Random.Range(0, spawnPoints.Count)];
            SpawnOneAt(p.position);
        }
        if (verbose) Debug.Log($"[Spawner] ForceSpawn {n} done", this);
    }

    void SpawnOneAt(Vector3 pos)
    {
        var go = Instantiate(enemyPrefab, pos, Quaternion.identity, enemiesRoot);
        go.name = "EnemyFinal";

        // 안전 보정: 태그/컴포넌트/FirePoint/총알 프리팹
        if (!go.CompareTag("Enemy")) go.tag = "Enemy";

        var hp = go.GetComponent<Health>(); if (!hp) { hp = go.AddComponent<Health>(); hp.maxHP = 100; hp.currentHP = 100; }

        var ew = go.GetComponent<EnemyWander>(); if (!ew) ew = go.AddComponent<EnemyWander>();
        ew.bulletResourcePath = "Z_EnemyBullet";     // 이름 고정
        ew.maxBulletsOnAir = 4;
        ew.fireInterval = 1.0f;
        ew.bulletSpeed = 9f;
        ew.bulletLife = 2.2f;

        // FirePoint 자동 보정
        var fp = go.GetComponentInChildren<Transform>(true);
        Transform found = null;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            if (t.name.Replace(" ", "").ToLower() == "firepoint") { found = t; break; }
        if (!found)
        {
            var f = new GameObject("FirePoint");
            f.transform.SetParent(go.transform);
            f.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            f.transform.localRotation = Quaternion.identity;
            ew.firePoint = f.transform;
        }
        else ew.firePoint = found;

        if (verbose) Debug.Log($"[Spawner] Spawn @ {pos} (alive→{CountAliveInScene() + 1}/{maxAlive})", this);
    }

    int CountAliveInScene()
    {
        var arr = Object.FindObjectsByType<EnemyWander>(FindObjectsSortMode.None);
        int c = 0; foreach (var e in arr) if (e && e.gameObject.activeInHierarchy) c++;
        return c;
    }

    void AutoCreatePoints()
    {
        var cam = Camera.main; if (!cam) { spawnPoints.Add(transform); return; }
        Vector3 p1 = cam.ViewportToWorldPoint(new Vector3(0.2f, 0.85f, -cam.transform.position.z));
        Vector3 p2 = cam.ViewportToWorldPoint(new Vector3(0.8f, 0.85f, -cam.transform.position.z));
        Vector3 p3 = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.95f, -cam.transform.position.z));
        spawnPoints.Clear();
        spawnPoints.Add(CreatePoint("EnemyPoint1", p1));
        spawnPoints.Add(CreatePoint("EnemyPoint2", p2));
        spawnPoints.Add(CreatePoint("EnemyPoint3", p3));
        if (verbose) Debug.Log("[Spawner] AutoCreatePoints 3개 생성", this);
    }

    Transform CreatePoint(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.position = pos;
        return go.transform;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var p in spawnPoints) if (p) Gizmos.DrawWireSphere(p.position, 0.25f);
    }
#endif
}
