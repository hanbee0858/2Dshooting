using UnityEngine;

[DisallowMultipleComponent]
public class PlayerShooting : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;            // 총구(자식에 FirePoint 권장)
    public GameObject bulletPrefab;        // 총알 프리팹(BulletBase 포함)

    [Header("Bullet")]
    public float bulletSpeed = 12f;
    public float bulletLife = 2f;
    public float bulletAccel = 0f;

    [Header("Control")]
    public KeyCode fireKey = KeyCode.Space;
    public bool autoBattleEnabled = false;
    public float autoFireInterval = 0.25f;
    float autoCd;

    [Header("Diagnostics")]
    public bool verboseLog = true;
    [Tooltip("Resources 폴더 아래 경로/이름 (예: Resources/Bullet.prefab)")]
    public string fallbackResourcePath = "Bullet";

    // 런타임 보호용 캐시(직렬화 X)
    GameObject _cachedBulletPrefab;
    Transform _cachedFirePoint;

    void Awake()
    {
        if (verboseLog) Debug.Log($"[PS/Awake] '{name}' id={GetInstanceID()}", this);

        // FirePoint 자동 탐색
        if (firePoint == null)
        {
            var fp = transform.Find("FirePoint");
            if (fp != null)
            {
                firePoint = fp;
                if (verboseLog) Debug.Log("[PS] FirePoint를 자식에서 자동 연결했습니다.", this);
            }
        }
        _cachedFirePoint = firePoint;

        // bulletPrefab 비어있으면 Resources 로드 시도
        if (bulletPrefab == null && !string.IsNullOrEmpty(fallbackResourcePath))
        {
            var res = Resources.Load<GameObject>(fallbackResourcePath);
            if (res != null)
            {
                bulletPrefab = res;
                if (verboseLog) Debug.Log($"[PS] Resources/{fallbackResourcePath}에서 bulletPrefab 자동 로드.", this);
            }
        }
        _cachedBulletPrefab = bulletPrefab;
    }

    void Start()
    {
        if (verboseLog)
            Debug.Log($"[PS/Start] fp={(firePoint ? firePoint.name : "NULL")}, bullet={(bulletPrefab ? bulletPrefab.name : "NULL")}, scene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}", this);
    }

    void Update()
    {
        if (Time.timeScale <= 0f) return;

        if (Input.GetKeyDown(fireKey))
            Fire();

        if (autoBattleEnabled)
        {
            autoCd -= Time.deltaTime;
            if (autoCd <= 0f)
            {
                Fire();
                autoCd = autoFireInterval;
            }
        }
    }

    public void Fire()
    {
        // 1) 런타임 중 끊긴 경우, 캐시로 복구
        if (bulletPrefab == null && _cachedBulletPrefab != null)
        {
            bulletPrefab = _cachedBulletPrefab;
            if (verboseLog) Debug.LogWarning("[PS] bulletPrefab이 런타임에 NULL → 캐시로 복구했습니다.", this);
        }
        if (firePoint == null && _cachedFirePoint != null)
        {
            firePoint = _cachedFirePoint;
            if (verboseLog) Debug.LogWarning("[PS] firePoint가 런타임에 NULL → 캐시로 복구했습니다.", this);
        }

        // 2) 그래도 bulletPrefab이 NULL이면, Resources에서 한 번 더 로드 시도
        if (bulletPrefab == null && !string.IsNullOrEmpty(fallbackResourcePath))
        {
            var res = Resources.Load<GameObject>(fallbackResourcePath);
            if (res != null)
            {
                bulletPrefab = res;
                _cachedBulletPrefab = res;
                if (verboseLog) Debug.LogWarning($"[PS] 런타임에 Resources/{fallbackResourcePath} 재로딩으로 복구했습니다.", this);
            }
        }

        // 최종 점검
        if (bulletPrefab == null || firePoint == null)
        {
            if (bulletPrefab == null && firePoint == null)
                Debug.LogError("[PlayerShooting] bulletPrefab과 firePoint가 모두 NULL입니다. Player 자식에 'FirePoint' 배치/연결 + 총알 프리팹 연결/Resources에 Bullet.prefab 두기.", this);
            else if (bulletPrefab == null)
                Debug.LogError("[PlayerShooting] bulletPrefab이 NULL입니다. 총알 프리팹(BulletBase + Rigidbody2D + Collider2D)을 연결하거나 Resources/Bullet.prefab을 준비하세요.", this);
            else
                Debug.LogError("[PlayerShooting] firePoint가 NULL입니다. Player 자식에 'FirePoint'를 만들고 연결하세요.", this);
            return;
        }

        if (verboseLog)
            Debug.Log($"[PS/Fire] bullet='{bulletPrefab.name}', fp='{firePoint.name}' on '{name}'", this);

        // 발사
        GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        var b = go.GetComponent<BulletBase>();
        if (b == null)
        {
            Debug.LogError("[PlayerShooting] bulletPrefab에 BulletBase가 없습니다!", go);
            return;
        }

        Vector2 dir = (Vector2)firePoint.up;
        b.Init(dir, bulletSpeed, bulletLife, transform, bulletAccel);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 에디터에서 값 수정 시 캐시 동기화
        _cachedBulletPrefab = bulletPrefab;
        _cachedFirePoint = firePoint;
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.up * 0.8f);
    }
#endif
}