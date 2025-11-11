using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyWander : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;                         // 자식 FirePoint
    public GameObject bulletPrefab;                      // Resources/Z_EnemyBullet (파란 큐브)
    public string bulletResourcePath = "Z_EnemyBullet";

    [Header("Move")]
    public float speed = 2.5f;
    public float wanderRadius = 6f;
    public Vector2 newTargetTimeRange = new Vector2(1.2f, 2.2f);
    public float arriveThreshold = 0.25f;
    public bool clampToCamera = true;

    [Header("Shoot")]
    public bool aimAtPlayer = true;
    public float fireInterval = 1.0f;
    public float bulletSpeed = 9f;
    public float bulletLife = 2.2f;
    public float bulletAccel = 0f;
    public int maxBulletsOnAir = 4;                   // 개별 상한
    public float fireJitter = 0f;

    [Header("Debug")]
    public bool verbose = false;

    Transform player;
    Vector3 startPos, targetPos;
    float changeTimer, fireCd;
    int myBullets;

    const float MIN_FIRE_INTERVAL = 0.3f;

    void Awake()
    {
        // 최소 물리 보정
        var rb = GetComponent<Rigidbody2D>(); if (!rb) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f;
        var col = GetComponent<Collider2D>(); col.isTrigger = true;

        firePoint = FindOrCreateFirePoint();

        // 슬롯이 씬 오브젝트를 물면 무효화하고 다시 로드
        if (bulletPrefab != null && bulletPrefab.scene.IsValid()) bulletPrefab = null;
        if (!bulletPrefab && !string.IsNullOrEmpty(bulletResourcePath))
            bulletPrefab = Resources.Load<GameObject>(bulletResourcePath);
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player"); if (p) player = p.transform;
        startPos = transform.position; PickNewTarget();

        fireInterval = Mathf.Max(MIN_FIRE_INTERVAL, fireInterval);
        maxBulletsOnAir = Mathf.Max(1, maxBulletsOnAir);

        if (verbose) Debug.Log($"[EW/Start] {name} fp={firePoint?.name}, bullet={bulletPrefab?.name}", this);
    }

    void Update()
    {
        MoveWander();
        ShootLoop();
    }

    // ---- Move ----
    void MoveWander()
    {
        changeTimer -= Time.deltaTime;
        if (changeTimer <= 0f || Vector2.Distance(transform.position, targetPos) <= arriveThreshold)
            PickNewTarget();

        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (dir.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }

        if (clampToCamera && Camera.main)
        {
            var cam = Camera.main;
            Vector3 min = cam.ViewportToWorldPoint(new Vector3(0.05f, 0.05f, -cam.transform.position.z));
            Vector3 max = cam.ViewportToWorldPoint(new Vector3(0.95f, 0.95f, -cam.transform.position.z));
            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, min.x, max.x);
            pos.y = Mathf.Clamp(pos.y, min.y, max.y);
            transform.position = pos;
        }
    }

    void PickNewTarget()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        targetPos = startPos + new Vector3(offset.x, offset.y, 0f);
        changeTimer = Random.Range(newTargetTimeRange.x, newTargetTimeRange.y);
    }

    // ---- Shoot ----
    void ShootLoop()
    {
        if (!firePoint || !bulletPrefab) return;

        // 개별/전역 탄 상한
        if (myBullets >= maxBulletsOnAir) return;
        if (BulletSimple.GlobalEnemyBullets >= BulletSimple.GlobalEnemyBulletsMax) return;

        fireCd -= Time.deltaTime;
        if (fireCd > 0f) return;

        Vector2 dir = (aimAtPlayer && player)
            ? (player.position - firePoint.position).normalized
            : (Vector2)firePoint.up;

        ShootOne(dir);

        fireCd = Mathf.Max(MIN_FIRE_INTERVAL, fireInterval + Random.Range(-fireJitter, fireJitter));
    }

    void ShootOne(Vector2 dir)
    {
        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        go.transform.SetParent(transform, worldPositionStays: true);

        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0, 0, ang);

        var b = go.GetComponent<BulletSimple>();
        if (!b)
        {
            // 방탄: 누락시 자동 추가 + 최소 물리 보정
            if (verbose) Debug.LogWarning("[EW] BulletSimple 누락 → 런타임 자동 추가", go);
            b = go.AddComponent<BulletSimple>();
            var rb = go.GetComponent<Rigidbody2D>(); if (!rb) { rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f; }
            var col = go.GetComponent<Collider2D>(); if (!col) { var c = go.AddComponent<CircleCollider2D>(); c.radius = 0.08f; c.isTrigger = true; } else { col.isTrigger = true; }
        }

        // 적탄은 주인 검사 + 전역 카운트
        b.requireEnemyOwner = true;
        b.Init(dir, bulletSpeed, bulletLife, bulletAccel, 15, "Enemy");

        // 개별 카운트 감소 훅
        var hook = go.AddComponent<ReturnHook>();
        hook.onDespawn = () => { myBullets = Mathf.Max(0, myBullets - 1); };

        myBullets++;
        if (verbose) Debug.Log($"[EW/Fire] {name} mine={myBullets}, global={BulletSimple.GlobalEnemyBullets}", this);
    }

    class ReturnHook : MonoBehaviour
    {
        public System.Action onDespawn;
        void OnDisable() => onDespawn?.Invoke();
        void OnDestroy() => onDespawn?.Invoke();
    }

    // ---- Util ----
    Transform FindOrCreateFirePoint()
    {
        if (firePoint) return firePoint;

        var fp = GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name.Replace(" ", "").Equals("FirePoint", System.StringComparison.OrdinalIgnoreCase));
        if (fp) return fp;

        var go = new GameObject("FirePoint");
        fp = go.transform; fp.SetParent(transform);
        fp.localPosition = new Vector3(0f, 0.5f, 0f);
        fp.localRotation = Quaternion.identity;
        return fp;
    }
}