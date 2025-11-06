using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;            // 비어있으면 Start에서 "Player" 태그로 자동 찾음
    public Transform firePoint;         // 비어있으면 자식에서 "FirePoint" 자동 탐색
    public GameObject bulletPrefab;     // 비어있으면 Resources/EnemyBullet 자동 로드

    [Header("Move")]
    public float moveSpeed = 2f;
    public float stopDistance = 3f;     // 이 거리 이내면 멈추고 사격

    [Header("Shoot")]
    public float bulletSpeed = 8f;
    public float bulletLife = 2f;
    public float bulletAccel = 0f;
    public float fireInterval = 1.2f;
    float fireCd;

    [Header("Diagnostics")]
    public bool verbose = true;
    public string fallbackBulletPath = "EnemyBullet"; // Resources/EnemyBullet.prefab

    void Awake()
    {
        // FirePoint 자동 탐색
        if (firePoint == null)
        {
            var fp = transform.Find("FirePoint");
            if (fp != null) firePoint = fp;
            else if (verbose) Debug.LogWarning("[Enemy] FirePoint가 없어 자식에서 찾을 수 없었습니다.", this);
        }

        // Bullet 자동 로드
        if (bulletPrefab == null && !string.IsNullOrEmpty(fallbackBulletPath))
        {
            var res = Resources.Load<GameObject>(fallbackBulletPath);
            if (res != null) bulletPrefab = res;
            else if (verbose) Debug.LogWarning($"[Enemy] Resources/{fallbackBulletPath} 로드 실패. 슬롯에 프리팹을 연결하세요.", this);
        }
    }

    void Start()
    {
        // Player 자동 탐색
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else if (verbose) Debug.LogWarning("[Enemy] 'Player' 태그 오브젝트를 찾지 못했습니다.", this);
        }

        if (verbose)
        {
            Debug.Log($"[Enemy/Start] player={(player ? player.name : "NULL")}, " +
                      $"fp={(firePoint ? firePoint.name : "NULL")}, bullet={(bulletPrefab ? bulletPrefab.name : "NULL")}", this);
        }
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;
        MoveLogic();
        ShootLogic();
    }

    void MoveLogic()
    {
        if (player == null) return;

        Vector2 toPlayer = (player.position - transform.position);
        float dist = toPlayer.magnitude;

        if (dist > stopDistance)
        {
            Vector2 dir = toPlayer.normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
        }

        // 바라보는 방향(선택)
        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, ang);
        }
    }

    void ShootLogic()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;

        fireCd -= Time.deltaTime;
        if (fireCd > 0f) return;

        Vector2 dir = ((Vector2)(player.position - firePoint.position)).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = (Vector2)firePoint.up;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // 총알을 진행 방향으로 회전(위=진행방향)
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, ang);

        var b = go.GetComponent<BulletBase>();
        if (b != null)
        {
            b.Init(dir, bulletSpeed, bulletLife, transform, bulletAccel);
        }
        else
        {
            Debug.LogError("[Enemy] bulletPrefab에 BulletBase가 없습니다!", go);
        }

        fireCd = fireInterval;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.up * 0.8f);
    }
#endif
}