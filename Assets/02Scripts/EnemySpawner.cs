using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 적 스폰러: 자동 포인트 생성, 초기 다중 스폰, 생존 수 제한, 주기 스폰.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Prefab & Points")]
    [FormerlySerializedAs("enemyPrefab")] public GameObject EnemyPrefab;   // Resources/EnemyFinal(파란 큐브)
    [FormerlySerializedAs("resourcePath")] public string ResourcePath = "EnemyFinal";
    [FormerlySerializedAs("spawnPoints")] public List<Transform> SpawnPoints = new();

    [Header("Rules")]
    [FormerlySerializedAs("spawnInterval")] public float SpawnInterval = 1.5f;
    [FormerlySerializedAs("maxAlive")] public int MaxAlive = 8;
    [FormerlySerializedAs("initialSpawnCount")] public int InitialSpawnCount = 4;

    [Header("Root (optional)")]
    [FormerlySerializedAs("enemiesRoot")] public Transform EnemiesRoot;

    [Header("Options")]
    [FormerlySerializedAs("spawnOnEnable")] public bool SpawnOnEnable = true;

    private Coroutine _loop;

    private void OnEnable()
    {
        if (EnemyPrefab == null && !string.IsNullOrEmpty(ResourcePath))
        {
            EnemyPrefab = Resources.Load<GameObject>(ResourcePath);
        }

        if (EnemyPrefab == null)
        {
            Debug.LogError("[Spawner] EnemyPrefab 미지정 또는 Resources/" + ResourcePath + " 없음.", this);
            return;
        }

        if (SpawnPoints.Count == 0)
        {
            foreach (Transform t in transform)
            {
                if (t != null)
                {
                    SpawnPoints.Add(t);
                }
            }
        }

        if (SpawnPoints.Count == 0)
        {
            AutoCreatePoints();
        }

        if (EnemiesRoot == null)
        {
            var root = GameObject.Find("Enemies_Container");
            if (root == null)
            {
                root = new GameObject("Enemies_Container");
            }
            EnemiesRoot = root.transform;
        }

        ForceSpawn(InitialSpawnCount);

        if (SpawnOnEnable)
        {
            if (_loop != null)
            {
                StopCoroutine(_loop);
            }

            _loop = StartCoroutine(SpawnLoop());
        }
    }

    private void OnDisable()
    {
        if (_loop != null)
        {
            StopCoroutine(_loop);
        }
    }

    private IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(SpawnInterval);
        while (true)
        {
            TrySpawnOne();
            yield return wait;
        }
    }

    private void TrySpawnOne()
    {
        int alive = CountAliveInScene();
        if (alive >= MaxAlive)
        {
            return;
        }

        var p = SpawnPoints[Random.Range(0, SpawnPoints.Count)];
        SpawnOneAt(p.position);
    }

    private void ForceSpawn(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var p = SpawnPoints[Random.Range(0, SpawnPoints.Count)];
            SpawnOneAt(p.position);
        }
    }

    private void SpawnOneAt(Vector3 pos)
    {
        var go = Object.Instantiate(EnemyPrefab, pos, Quaternion.identity, EnemiesRoot);
        go.name = "EnemyFinal";

        if (!go.CompareTag("Enemy"))
        {
            go.tag = "Enemy";
        }

        var hp = go.GetComponent<Health>();
        if (hp == null)
        {
            hp = go.AddComponent<Health>();
            hp.maxHP = 100;
            hp.currentHP = 100;
        }

        var ew = go.GetComponent<EnemyWander>();
        if (ew == null)
        {
            ew = go.AddComponent<EnemyWander>();
        }

        ew.BulletResourcePath = "Z_EnemyBullet";
        ew.MaxBulletsOnAir = 4;
        ew.FireInterval = 1.0f;
        ew.BulletSpeed = 9f;
        ew.BulletLife = 2.2f;
        ew.ClampToCamera = true;
        ew.WanderRadius = 6f;

        // FirePoint 보정
        Transform found = null;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Replace(" ", "").ToLower() == "firepoint")
            {
                found = t;
                break;
            }
        }

        if (found == null)
        {
            var f = new GameObject("FirePoint");
            f.transform.SetParent(go.transform);
            f.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            f.transform.localRotation = Quaternion.identity;
            ew.FirePoint = f.transform;
        }
        else
        {
            ew.FirePoint = found;
        }
    }

    private int CountAliveInScene()
    {
        var arr = Object.FindObjectsByType<EnemyWander>(FindObjectsSortMode.None);
        int c = 0;
        foreach (var e in arr)
        {
            if (e != null && e.gameObject.activeInHierarchy)
            {
                c++;
            }
        }
        return c;
    }

    private void AutoCreatePoints()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            SpawnPoints.Add(transform);
            return;
        }

        Vector3 p1 = cam.ViewportToWorldPoint(new Vector3(0.2f, 0.85f, -cam.transform.position.z));
        Vector3 p2 = cam.ViewportToWorldPoint(new Vector3(0.8f, 0.85f, -cam.transform.position.z));
        Vector3 p3 = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.95f, -cam.transform.position.z));

        SpawnPoints.Clear();
        SpawnPoints.Add(CreatePoint("EnemyPoint1", p1));
        SpawnPoints.Add(CreatePoint("EnemyPoint2", p2));
        SpawnPoints.Add(CreatePoint("EnemyPoint3", p3));
    }

    private Transform CreatePoint(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.position = pos;
        return go.transform;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var p in SpawnPoints)
        {
            if (p != null)
            {
                Gizmos.DrawWireSphere(p.position, 0.25f);
            }
        }
    }
#endif
}

