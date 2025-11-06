using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;              // 비어있으면 Tag=Player 자동 탐색
    public Transform firePoint;           // 비어있으면 자식 "FirePoint" 자동 탐색/생성
    public GameObject bulletPrefab;       // 비어있으면 Resources/EnemyBullet 로드

    [Header("Move")]
    public float moveSpeed = 2f;
    public float stopDistance = 3f;       // 이 이내면 멈추고 사격

    [Header("Shoot")]
    public float bulletSpeed = 8f;
    public float bulletLife = 2f;
    public float bulletAccel = 0f;
    public float fireInterval = 1.0f;
    float fireCd;

    [Header("Debug")]
    public bool verbose = true;
    public string fallbackBulletPath = "EnemyBullet"; // Assets/Resources/EnemyBullet.prefab

    void Awake()
    {
        // 💥 물리 밀림 차단: 적은 스크립트로만 이동
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // 🔎 FirePoint 확보
        if (firePoint == null)
        {
            var fp = transform.Find("FirePoint");
            if (fp != null) firePoint = fp;
            else
            {
                // 자동 생성(앞쪽 0.5)
                var go = new GameObject("FirePoint");
                firePoint = go.transform;
                firePoint.SetParent(transform);
                firePoint.localPosition = new Vector3(0f, 0.5f, 0f);
                firePoint.localRotation = Quaternion.identity;
                if (verbose) Debug.LogWarning("[Enemy] FirePoint가 없어 자동 생성했습니다.", this);
            }
        }

        // 🔎 총알 확보
        if (bulletPrefab == null && !string.IsNullOrEmpty(fallbackBulletPath))
        {
            var res = Resources.Load<GameObject>(fallbackBulletPath);
            if (res != null) bulletPrefab = res;
        }
    }

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (verbose)
            Debug.Log($"[Enemy/Start] player={(player ? player.name : "NULL")}, fp={(firePoint ? firePoint.name : "NULL")}, bullet={(bulletPrefab ? bulletPrefab.name : "NULL")}", this);
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        MoveLogic();
        ShootLogic();
    }

    void MoveLogic()
    {
        if (!player) return;

        Vector2 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist > stopDistance)
        {
            // 스크립트 이동(물리와 무관) → 플레이어가 움직여도 끌려가지 않음
            Vector2 dir = toPlayer.normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        }

        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, ang);
        }
    }

    void ShootLogic()
    {
        if (!player || !firePoint || !bulletPrefab) return;

        fireCd -= Time.deltaTime;
        if (fireCd > 0f) return;

        Vector2 dir = ((Vector2)(player.position - firePoint.position)).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = (Vector2)firePoint.up;

        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, ang);

        var b = go.GetComponent<BulletBase>();
        if (b != null) b.Init(dir, bulletSpeed, bulletLife, transform, bulletAccel);
        else Debug.LogError("[Enemy] bulletPrefab에 BulletBase가 없습니다!", go);

        fireCd = fireInterval;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 에디터에서 잘못 끼워 넣은 설정 자동 교정
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f; }
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        if (moveSpeed < 0f) moveSpeed = 0f;
        if (stopDistance < 0.1f) stopDistance = 0.1f;
        if (fireInterval < 0.05f) fireInterval = 0.05f;
    }
#endif
}