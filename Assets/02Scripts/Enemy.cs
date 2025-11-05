using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("체력")] public float maxHP = 100f;
    float hp;

    [Header("이동(카메라 내부 랜덤 배회)")]
    public float moveSpeed = 2.4f;
    public float retargetEvery = 1.5f;   // 목표 갱신 주기
    public float screenPadding = 0.8f;   // 화면 가장자리 여백
    Vector2 targetPos; float retargetAt;

    [Header("발사")]
    public GameObject enemyBulletPrefab;
    public string enemyBulletResourcePath = "EnemyBullet";
    public float fireRate = 1.1f;
    public float fireRandom = 0.35f;
    public float enemyBulletSpeed = 6f;
    public float enemyBulletDamage = 20f;

    [Header("동시 활성 제한")]
    public int maxActiveEnemyBullets = 60;

    Camera cam; Vector2 camMin, camMax; // 화면 경계

    void Awake()
    {
        hp = maxHP;
        if (!enemyBulletPrefab)
            enemyBulletPrefab = Resources.Load<GameObject>(enemyBulletResourcePath);

        cam = Camera.main;
        UpdateScreenBounds();
        CameraShake.Ensure();
    }

    void Start()
    {
        PickTargetInsideScreen(true);
        ScheduleNextFire();
    }

    void Update()
    {
        UpdateScreenBounds();

        // 이동
        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // 목표 도달/코너 끼임/경계 접근 시 재선정
        bool nearTarget = (Vector2.Distance(transform.position, targetPos) < 0.15f);
        bool nearEdge = IsNearEdge(transform.position);
        if (Time.time >= retargetAt || nearTarget || nearEdge)
            PickTargetInsideScreen(false);

        // 화면 안으로 강제 클램프(끈질기게 밖으로 못 나가게)
        ClampToScreen();

        // 발사
        if (Time.time >= _nextFire) { FireOnce(); ScheduleNextFire(); }
    }

    // --- 화면 경계 계산 ---
    void UpdateScreenBounds()
    {
        if (!cam) return;
        var v0 = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        var v1 = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        camMin = v0; camMax = v1;
    }

    bool IsNearEdge(Vector2 p)
    {
        return p.x < camMin.x + screenPadding || p.x > camMax.x - screenPadding ||
               p.y < camMin.y + screenPadding || p.y > camMax.y - screenPadding;
    }

    void ClampToScreen()
    {
        var p = transform.position;
        p.x = Mathf.Clamp(p.x, camMin.x + screenPadding, camMax.x - screenPadding);
        p.y = Mathf.Clamp(p.y, camMin.y + screenPadding, camMax.y - screenPadding);
        transform.position = p;
    }

    void PickTargetInsideScreen(bool first)
    {
        // 화면 안에서만 목표 선정(여백 포함)
        float x = Random.Range(camMin.x + screenPadding, camMax.x - screenPadding);
        float y = Random.Range(camMin.y + screenPadding, camMax.y - screenPadding);
        targetPos = new Vector2(x, y);

        // 다음 목표 선정 시간 예약(살짝 랜덤)
        retargetAt = Time.time + retargetEvery + (first ? 0f : Random.Range(0f, 0.6f));
    }

    // --- 발사 ---
    float _nextFire;
    void ScheduleNextFire() => _nextFire = Time.time + fireRate + Random.Range(0f, fireRandom);

    void FireOnce()
    {
        if (!enemyBulletPrefab) return;
        if (SimplePool.ActiveCount(enemyBulletPrefab) >= maxActiveEnemyBullets) return;

        var go = SimplePool.Get(enemyBulletPrefab, transform.position, Quaternion.identity);
        go.GetComponent<BulletBase>()?.Init("Enemy", Vector2.down, enemyBulletSpeed, enemyBulletDamage, enemyBulletPrefab);
    }

    // --- 데미지 ---
    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0f)
        {
            CameraShake.ShakeNow(0.25f, 0.25f);
            Destroy(gameObject);
        }
        else
        {
            CameraShake.ShakeNow(0.10f, 0.12f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraShake.ShakeNow(0.25f, 0.22f);
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}