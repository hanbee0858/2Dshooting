using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;                       // Player 자식 FirePoint
    public GameObject bulletPrefab;                    // Resources/PlayerBullet 또는 PlayerBulletSimple
    public string bulletResourcePath = "PlayerBullet"; // 프리팹 이름에 맞춰주세요 ("PlayerBullet" or "PlayerBulletSimple")

    [Header("Control")]
    public KeyCode fireKey = KeyCode.Space;
    public KeyCode autoOnKey = KeyCode.Alpha1;
    public KeyCode autoOffKey = KeyCode.Alpha2;

    [Header("Bullet")]
    public float bulletSpeed = 6f;
    public float bulletLife = 2f;
    public float bulletAccel = 0f;
    public float fireInterval = 0.15f;
    public bool autoBattleEnabled = false;

    float cd;

    void Awake()
    {
        // FirePoint 자동 복구(없으면 생성)
        if (!firePoint)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
                if (t.name.Replace(" ", "").Equals("FirePoint", System.StringComparison.OrdinalIgnoreCase)) { firePoint = t; break; }
            if (!firePoint)
            {
                var go = new GameObject("FirePoint");
                firePoint = go.transform; firePoint.SetParent(transform);
                firePoint.localPosition = new Vector3(0f, 0.5f, 0f);
                firePoint.localRotation = Quaternion.identity;
            }
        }

        // 프리팹 없으면 Resources에서 로드
        if (!bulletPrefab && !string.IsNullOrEmpty(bulletResourcePath))
        {
            var res = Resources.Load<GameObject>(bulletResourcePath);
            if (res) bulletPrefab = res;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(autoOnKey)) autoBattleEnabled = true;
        if (Input.GetKeyDown(autoOffKey)) autoBattleEnabled = false;

        cd -= Time.deltaTime;
        if ((Input.GetKey(fireKey) || autoBattleEnabled) && cd <= 0f)
        {
            if (!firePoint || !bulletPrefab)
            {
                Debug.LogWarning("[PS] Fire 실패: firePoint 또는 bulletPrefab이 null", this);
                return;
            }
            Fire();
            cd = fireInterval;
        }
    }

    void Fire()
    {
        Vector2 dir = firePoint.up;

        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0, 0, ang);

        // 🔧 방탄: BulletSimple 없으면 자동으로 붙이고 필수 컴포넌트 보정
        var b = go.GetComponent<BulletSimple>();
        if (!b)
        {
            Debug.LogWarning("[PS] BulletSimple 누락 → 런타임 자동 추가", go);
            b = go.AddComponent<BulletSimple>();

            var rb = go.GetComponent<Rigidbody2D>();
            if (!rb) { rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f; }

            var col = go.GetComponent<Collider2D>();
            if (!col)
            {
                var c = go.AddComponent<CircleCollider2D>();
                c.radius = 0.08f; c.isTrigger = true;
            }
            else col.isTrigger = true;
        }

        // 플레이어 탄 파라미터
        b.Init(dir, bulletSpeed, bulletLife, bulletAccel, 15, "Player");
    }
}