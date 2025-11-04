using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("가속(빠르게 체감)")]
    public float startSpeed = 2f;
    public float endSpeed = 14f;
    public float accelTime = 1.2f;

    [Header("기타")]
    public float lifeTime = 5f;

    Rigidbody2D rb;
    Vector2 dir = Vector2.up; // 기본 위쪽
    float t;

    public void Init(Vector2 direction)
    {
        dir = direction.sqrMagnitude > 0 ? direction.normalized : Vector2.up;
    }

    void Awake()
    {
        // 혹시 Player에 붙었으면 즉시 중단(문제의 핵심을 예방)
        if (GetComponent<PlayerShooting>() != null)
        {
            Debug.LogError("[Bullet] Player에 붙어있습니다. 총알 프리팹에만 붙이세요.");
            enabled = false; return;
        }

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;        // 총알은 중력 X
        rb.freezeRotation = true;
    }

    void OnEnable() => Destroy(gameObject, lifeTime);

    void FixedUpdate()
    {
        if (!enabled) return;

        t += Time.fixedDeltaTime;
        float k = Mathf.Clamp01(t / accelTime);
        float speed = Mathf.Lerp(startSpeed, endSpeed, k);

        rb.velocity = dir * speed;
    }
}