using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [Header("Runtime")]
    public Vector2 direction = Vector2.up;
    public float speed = 6f;        // 초당
    public float life = 2f;         // 초
    public float accel = 0f;        // 초당 가속(+면 가속, 0 권장)
    public float maxSpeed = 15f;
    public Transform owner;         // 발사자 (Enemy/Player 태그 판별용)

    [Header("Collision")]
    public LayerMask ignoreLayers;  // 경계/벽 레이어 체크해두면 충돌 무시

    float t;
    GameObject _prefabRef;          // 풀 반납용 원본 참조

    // NEW: prefabRef 추가됨
    public void Init(Vector2 dir, float spd, float lf, Transform own, float ac, GameObject prefabRef)
    {
        direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.up;
        speed = spd; life = lf; owner = own; accel = ac; t = 0f; _prefabRef = prefabRef;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        t += dt;
        if (t >= life) { Despawn(); return; }

        if (accel != 0f)
        {
            speed += accel * dt;
            speed = Mathf.Clamp(speed, 0f, maxSpeed);
        }

        transform.position += (Vector3)(direction * speed * dt);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 1) 레이어 마스크로 무시
        if ((ignoreLayers.value & (1 << other.gameObject.layer)) != 0) return;

        if (!owner) { Despawn(); return; }

        bool fromEnemy = owner.CompareTag("Enemy");

        // 2) 아군/적군만 타격
        if (fromEnemy && other.CompareTag("Player")) { /* TODO: 데미지 */ Despawn(); return; }
        if (!fromEnemy && other.CompareTag("Enemy")) { /* TODO: 데미지 */ Despawn(); return; }

        // 나머지는 관통
    }

    void Despawn()
    {
        if (_prefabRef != null && BulletPool.I) BulletPool.I.Return(_prefabRef, gameObject);
        else Destroy(gameObject);
    }
}