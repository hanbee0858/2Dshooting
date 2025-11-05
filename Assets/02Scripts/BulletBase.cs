using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [Header("Move")]
    public float speed = 10f;          // 기본 속도
    public float acceleration = 0f;    // 가속(선택)
    public Vector2 direction = Vector2.up; // 이동 방향(정규화 예정)

    [Header("Lifetime")]
    public float lifeTime = 2f;        // 수명(초)
    private float _timer;

    [Header("Owner")]
    public Transform owner;            // 발사자(아군 판정 등에 사용)

    private Rigidbody2D _rb;

    // ─────────────────────────────────────────────
    // Init 오버로드: 호출처가 몇 개의 인자를 주든 대응합니다.
    // (예전 오류 "Init takes 5 arguments"도 이 5개짜리 시그니처가 처리)
    // ─────────────────────────────────────────────
    public void Init(Vector2 dir)
    {
        direction = dir;
        NormalizeDir();
        ApplyVelocityImmediate();
    }

    public void Init(Vector2 dir, float spd)
    {
        direction = dir;
        speed = spd;
        NormalizeDir();
        ApplyVelocityImmediate();
    }

    public void Init(Vector2 dir, float spd, float life)
    {
        direction = dir;
        speed = spd;
        lifeTime = life;
        NormalizeDir();
        ApplyVelocityImmediate();
        _timer = lifeTime;
    }

    public void Init(Vector2 dir, float spd, float life, Transform owner)
    {
        direction = dir;
        speed = spd;
        lifeTime = life;
        this.owner = owner;
        NormalizeDir();
        ApplyVelocityImmediate();
        _timer = lifeTime;
    }

    public void Init(Vector2 dir, float spd, float life, Transform owner, float accel)
    {
        direction = dir;
        speed = spd;
        lifeTime = life;
        this.owner = owner;
        acceleration = accel;
        NormalizeDir();
        ApplyVelocityImmediate();
        _timer = lifeTime;
    }

    // ─────────────────────────────────────────────

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        // 새로 켜질 때 수명 초기화
        _timer = lifeTime;
        NormalizeDir();
        ApplyVelocityImmediate();
    }

    void Update()
    {
        // 수명 감소
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Despawn();
            return;
        }

        // 가속이 있으면 속도 증가
        if (acceleration != 0f)
        {
            speed += acceleration * Time.deltaTime;
            if (speed < 0f) speed = 0f;
            ApplyVelocityImmediate();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 자기 자신(발사자)과의 충돌은 무시
        if (owner != null && other.transform == owner) return;

        // 필요 시 태그로 필터링하려면 아래를 사용:
        // if (CompareTag("PlayerBullet") && other.CompareTag("Player")) return;
        // if (CompareTag("EnemyBullet")  && other.CompareTag("Enemy"))  return;

        Despawn();
    }

    void OnBecameInvisible()
    {
        // 화면 밖으로 나가면 제거(원치 않으면 이 메서드 삭제)
        Despawn();
    }

    // ─────────────────────────────────────────────
    void NormalizeDir()
    {
        if (direction.sqrMagnitude > 0.0001f)
            direction = direction.normalized;
        else
            direction = Vector2.up; // 기본값
    }

    void ApplyVelocityImmediate()
    {
        if (_rb != null)
            _rb.linearVelocity = direction * speed;
        else
            transform.Translate((Vector3)(direction * speed * Time.deltaTime), Space.World);
    }

    void Despawn()
    {
        // 풀을 쓰면 풀로 반환하도록 교체하세요.
        Destroy(gameObject);
    }
}