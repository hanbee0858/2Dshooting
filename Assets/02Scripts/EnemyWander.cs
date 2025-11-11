using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 적 랜덤 배회 + 플레이어 조준 사격. 화면 이탈 방지, 탄 제한(개별/전역).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyWander : MonoBehaviour
{
    [Header("Refs")]
    [FormerlySerializedAs("firePoint")] public Transform FirePoint;
    [FormerlySerializedAs("bulletPrefab")] public GameObject BulletPrefab;
    [FormerlySerializedAs("bulletResourcePath")] public string BulletResourcePath = "Z_EnemyBullet";

    [Header("Move")]
    [FormerlySerializedAs("speed")] public float Speed = 2.5f;
    [FormerlySerializedAs("wanderRadius")] public float WanderRadius = 6f;
    [FormerlySerializedAs("newTargetTimeRange")] public Vector2 NewTargetTimeRange = new(1.2f, 2.2f);
    [FormerlySerializedAs("arriveThreshold")] public float ArriveThreshold = 0.25f;
    [FormerlySerializedAs("clampToCamera")] public bool ClampToCamera = true;

    [Header("Shoot")]
    [FormerlySerializedAs("aimAtPlayer")] public bool AimAtPlayer = true;
    [FormerlySerializedAs("fireInterval")] public float FireInterval = 1.0f;
    [FormerlySerializedAs("bulletSpeed")] public float BulletSpeed = 9f;
    [FormerlySerializedAs("bulletLife")] public float BulletLife = 2.2f;
    [FormerlySerializedAs("bulletAccel")] public float BulletAccel = 0f;
    [FormerlySerializedAs("maxBulletsOnAir")] public int MaxBulletsOnAir = 4;
    [FormerlySerializedAs("fireJitter")] public float FireJitter = 0f;

    private Transform _player;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _changeTimer;
    private float _fireCd;
    private int _myBullets;

    private const float MinFireInterval = 0.3f;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        FirePoint = FindOrCreateFirePoint();

        if (BulletPrefab != null && BulletPrefab.scene.IsValid())
            BulletPrefab = null;

        if (BulletPrefab == null && !string.IsNullOrEmpty(BulletResourcePath))
            BulletPrefab = Resources.Load<GameObject>(BulletResourcePath);
    }

    private void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;

        _startPos = transform.position;
        PickNewTarget();

        FireInterval = Mathf.Max(MinFireInterval, FireInterval);
        MaxBulletsOnAir = Mathf.Max(1, MaxBulletsOnAir);
    }

    private void Update()
    {
        MoveWander();
        ShootLoop();
    }

    private void MoveWander()
    {
        _changeTimer -= Time.deltaTime;
        if (_changeTimer <= 0f || Vector2.Distance(transform.position, _targetPos) <= ArriveThreshold)
            PickNewTarget();

        Vector3 dir = (_targetPos - transform.position).normalized;
        transform.position += dir * Speed * Time.deltaTime;

        if (dir.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }

        KeepInsideCamera();
    }

    /// <summary>다음 임의 목적지 설정.</summary>
    private void PickNewTarget()
    {
        Vector2 offset = Random.insideUnitCircle * WanderRadius;
        _targetPos = _startPos + new Vector3(offset.x, offset.y, 0f);
        _changeTimer = Random.Range(NewTargetTimeRange.x, NewTargetTimeRange.y);
    }

    private void ShootLoop()
    {
        if (FirePoint == null || BulletPrefab == null) return;
        if (_myBullets >= MaxBulletsOnAir) return;
        if (BulletSimple.GlobalEnemyBullets >= BulletSimple.GlobalEnemyBulletsMax) return;

        _fireCd -= Time.deltaTime;
        if (_fireCd > 0f) return;

        Vector2 dir = (AimAtPlayer && _player != null)
            ? (Vector2)((_player.position - FirePoint.position).normalized)
            : (Vector2)FirePoint.up;

        ShootOne(dir);
        _fireCd = Mathf.Max(MinFireInterval, FireInterval + Random.Range(-FireJitter, FireJitter));
    }

    private void ShootOne(Vector2 dir)
    {
        var go = Instantiate(BulletPrefab, FirePoint.position, Quaternion.identity);
        go.transform.SetParent(transform, true);

        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0, 0, ang);

        var b = go.GetComponent<BulletSimple>();
        if (b == null)
        {
            b = go.AddComponent<BulletSimple>();
            var rb = go.GetComponent<Rigidbody2D>() ?? go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = go.GetComponent<Collider2D>() ?? go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            if (col is CircleCollider2D cc) cc.radius = 0.08f;
        }

        b.RequireEnemyOwner = true;
        b.Init(dir, BulletSpeed, BulletLife, BulletAccel, 15, "Enemy");

        var hook = go.AddComponent<ReturnHook>();
        hook.OnDespawn = () => { _myBullets = Mathf.Max(0, _myBullets - 1); };
        _myBullets++;
    }

    private class ReturnHook : MonoBehaviour
    {
        public System.Action OnDespawn;
        private void OnDisable() => OnDespawn?.Invoke();
        private void OnDestroy() => OnDespawn?.Invoke();
    }

    private void KeepInsideCamera()
    {
        if (!ClampToCamera) return;
        if (!TryGetCameraBounds(out var min, out var max)) return;

        var pos = transform.position;
        bool outX = pos.x < min.x || pos.x > max.x;
        bool outY = pos.y < min.y || pos.y > max.y;
        if (!(outX || outY)) return;

        var cam = Camera.main;
        var center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -cam.transform.position.z));
        _targetPos = new Vector3(
            Mathf.Clamp(center.x + Random.Range(-WanderRadius, WanderRadius), min.x, max.x),
            Mathf.Clamp(center.y + Random.Range(-WanderRadius, WanderRadius), min.y, max.y),
            0f
        );
        _changeTimer = Random.Range(NewTargetTimeRange.x, NewTargetTimeRange.y);

        pos.x = Mathf.Clamp(pos.x, min.x, max.x);
        pos.y = Mathf.Clamp(pos.y, min.y, max.y);
        transform.position = pos;
    }

    private bool TryGetCameraBounds(out Vector3 min, out Vector3 max)
    {
        min = max = Vector3.zero;
        var cam = Camera.main;
        if (cam == null) return false;

        min = cam.ViewportToWorldPoint(new Vector3(0.05f, 0.05f, -cam.transform.position.z));
        max = cam.ViewportToWorldPoint(new Vector3(0.95f, 0.95f, -cam.transform.position.z));
        return true;
    }

    private Transform FindOrCreateFirePoint()
    {
        if (FirePoint != null) return FirePoint;

        var fp = GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name.Replace(" ", "").Equals("FirePoint", System.StringComparison.OrdinalIgnoreCase));
        if (fp != null) return fp;

        var go = new GameObject("FirePoint");
        fp = go.transform;
        fp.SetParent(transform);
        fp.localPosition = new Vector3(0f, 0.5f, 0f);
        fp.localRotation = Quaternion.identity;
        return fp;
    }
}