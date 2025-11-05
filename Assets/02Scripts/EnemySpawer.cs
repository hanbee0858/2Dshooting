using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 1.5f;
    public int maxAlive = 15;
    public float padding = 0.8f;
    public bool logWhyNotSpawn = true;

    float t;

    void Update()
    {
        t += Time.deltaTime;
        if (t < spawnInterval) return;
        t = 0f;

        if (!enemyPrefab) { if (logWhyNotSpawn) Debug.LogError("[Spawner] enemyPrefab 미지정"); return; }

        int alive = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        if (alive >= maxAlive)
        {
            if (logWhyNotSpawn) Debug.Log($"[Spawner] 상한 도달: {alive}/{maxAlive}");
            return;
        }

        var cam = Camera.main; if (!cam) { if (logWhyNotSpawn) Debug.LogError("[Spawner] Main Camera 없음"); return; }

        var min = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        var max = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float x = Random.Range(min.x + padding, max.x - padding);
        float y = Random.Range(min.y + padding, max.y - padding);

        Instantiate(enemyPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        if (logWhyNotSpawn) Debug.Log($"[Spawner] Spawn! 현재 {alive + 1}/{maxAlive}");
    }
}
