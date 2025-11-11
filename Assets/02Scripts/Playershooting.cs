using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 플레이어 사격(수동/자동). FirePoint 자동 복구, 프리팹 자동 로드, 누락시 런타임 보정 포함.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Refs")]
    [FormerlySerializedAs("firePoint")] public Transform FirePoint;     // Player 자식. 없으면 생성.
    [FormerlySerializedAs("bulletPrefab")] public GameObject BulletPrefab; // Resources/PlayerBullet
    [FormerlySerializedAs("bulletResourcePath")] public string BulletResourcePath = "PlayerBullet";

    [Header("Control")]
    [FormerlySerializedAs("fireKey")] public KeyCode FireKey = KeyCode.Space;
    [FormerlySerializedAs("autoOnKey")] public KeyCode AutoOnKey = KeyCode.Alpha1;
    [FormerlySerializedAs("autoOffKey")] public KeyCode AutoOffKey = KeyCode.Alpha2;

    [Header("Bullet")]
    [FormerlySerializedAs("bulletSpeed")] public float BulletSpeed = 6f;
    [FormerlySerializedAs("bulletLife")] public float BulletLife = 2f;
    [FormerlySerializedAs("bulletAccel")] public float BulletAccel = 0f;
    [FormerlySerializedAs("fireInterval")] public float FireInterval = 0.15f;

    [Header("State")]
    [FormerlySerializedAs("autoBattleEnabled")] public bool AutoBattleEnabled = false;

    private float _cd;

    private void Awake()
    {
        // FirePoint 자동 복구
        if (FirePoint == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Replace(" ", "").Equals("FirePoint", System.StringComparison.OrdinalIgnoreCase))
                {
                    FirePoint = t;
                    break;
                }
            }

            if (FirePoint == null)
            {
                var go = new GameObject("FirePoint");
                FirePoint = go.transform;
                FirePoint.SetParent(transform);
                FirePoint.localPosition = new Vector3(0f, 0.5f, 0f);
                FirePoint.localRotation = Quaternion.identity;
            }
        }

        // 프리팹 자동 로드
        if (BulletPrefab == null && !string.IsNullOrEmpty(BulletResourcePath))
        {
            var res = Resources.Load<GameObject>(BulletResourcePath);
            if (res != null)
            {
                BulletPrefab = res;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(AutoOnKey))
        {
            AutoBattleEnabled = true;
        }

        if (Input.GetKeyDown(AutoOffKey))
        {
            AutoBattleEnabled = false;
        }

        _cd -= Time.deltaTime;

        if ((Input.GetKey(FireKey) || AutoBattleEnabled) && _cd <= 0f)
        {
            if (FirePoint == null || BulletPrefab == null)
            {
                return;
            }

            Fire();
            _cd = FireInterval;
        }
    }

    /// <summary>한 발 발사(누락 시 런타임 보정 포함).</summary>
    public void Fire()
    {
        Vector2 dir = FirePoint.up;

        var go = Instantiate(BulletPrefab, FirePoint.position, Quaternion.identity);
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0, 0, ang);

        // 방탄: BulletSimple 없으면 자동 추가 + 기본 물리 세팅
        var b = go.GetComponent<BulletSimple>();
        if (b == null)
        {
            b = go.AddComponent<BulletSimple>();

            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }

            var col = go.GetComponent<Collider2D>();
            if (col == null)
            {
                var c = go.AddComponent<CircleCollider2D>();
                c.radius = 0.08f;
                c.isTrigger = true;
            }
            else
            {
                col.isTrigger = true;
            }
        }

        // 플레이어 탄 초기화
        b.RequireEnemyOwner = false;
        b.Init(dir, BulletSpeed, BulletLife, BulletAccel, 15, "Player");
    }
}