using UnityEngine;

public class EnemyReplicator : MonoBehaviour
{
    [Header("자기 자리에서 적 복제")]
    public GameObject enemyPrefab;          // ← ✅ 이게 Inspector에서 보여야 해요!
    public float reproduceInterval = 5f;    // 몇 초마다 복제
    public float jitter = 0.8f;             // 랜덤 지연
    public float offset = 0.25f;            // 살짝 위치 어긋나게

    [Header("안전장치")]
    public int maxAlive = 20;               // 한 번에 존재 가능한 전체 적 수
    public int maxGenerations = 3;          // 세대 제한
    [HideInInspector] public int generation = 0;

    float nextReproduceAt;

    void Start()
    {
        ScheduleNext();
    }

    void Update()
    {
        if (Time.time < nextReproduceAt) return;

        int alive = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
        if (alive < maxAlive && generation < maxGenerations)
        {
            SpawnChild();
        }

        ScheduleNext();
    }

    void ScheduleNext()
    {
        nextReproduceAt = Time.time + reproduceInterval + Random.Range(0f, jitter);
    }

    void SpawnChild()
    {
        if (!enemyPrefab) return;

        Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle.normalized * offset);

        var cam = Camera.main;
        if (cam)
        {
            var min = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
            var max = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
            pos.x = Mathf.Clamp(pos.x, min.x + 0.5f, max.x - 0.5f);
            pos.y = Mathf.Clamp(pos.y, min.y + 0.5f, max.y - 0.5f);
        }

        GameObject child = Instantiate(enemyPrefab, pos, Quaternion.identity);
        var rep = child.GetComponent<EnemyReplicator>();
        if (rep)
        {
            rep.generation = this.generation + 1;
            rep.enemyPrefab = this.enemyPrefab;
            rep.maxAlive = this.maxAlive;
            rep.maxGenerations = this.maxGenerations;
        }
    }
}
