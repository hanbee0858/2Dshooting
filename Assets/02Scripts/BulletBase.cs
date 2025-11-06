using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [Header("Move")]
    public float speed = 10f;
    public float acceleration = 0f;
    public Vector2 direction = Vector2.up;

    [Header("Lifetime")]
    public float lifeTime = 2f;
    float timer;

    [Header("Owner")]
    public Transform owner;

    Rigidbody2D rb;

    // Init 오버로드(호출부 타입/개수 달라도 OK)
    public void Init(Vector2 dir) { direction = SafeDir(dir); }
    public void Init(Vector2 dir, float spd) { direction = SafeDir(dir); speed = spd; }
    public void Init(Vector2 dir, float spd, float life) { direction = SafeDir(dir); speed = spd; lifeTime = life; }
    public void Init(Vector2 dir, float spd, float life, Transform owner)
    { direction = SafeDir(dir); speed = spd; lifeTime = life; this.owner = owner; }
    public void Init(Vector2 dir, float spd, float life, Transform owner, float accel)
    { direction = SafeDir(dir); speed = spd; lifeTime = life; this.owner = owner; acceleration = accel; }

    void Awake() { rb = GetComponent<Rigidbody2D>(); }
    void OnEnable()
    {
        timer = lifeTime;
        direction = SafeDir(direction);
        ApplyVelocity();
    }

    void Update()
    {
        // 수명 — 타임스케일 영향 받도록(deltaTime) 변경 (일시정지땐 멈추게)
        timer -= Time.deltaTime;
        if (timer <= 0f) { Destroy(gameObject); return; }

        // 가속
        if (acceleration != 0f)
        {
            speed += acceleration * Time.deltaTime;
            if (speed < 0f) speed = 0f;
        }

        ApplyVelocity();
    }

    void ApplyVelocity()
    {
        if (rb != null) rb.linearVelocity = direction * speed;
        else transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.transform == owner) return;
        Destroy(gameObject);
    }

    void OnBecameInvisible() { Destroy(gameObject); }

    Vector2 SafeDir(Vector2 dir) => (dir.sqrMagnitude > 0.0001f) ? dir.normalized : Vector2.up;
}
