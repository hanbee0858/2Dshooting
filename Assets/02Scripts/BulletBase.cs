using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(PoolAutoReturn))]
public class BulletBase : MonoBehaviour
{
    public string owner = "Player";        // "Player" or "Enemy"
    public float speed = 12f;
    public Vector2 dir = Vector2.up;
    public float lifeTime = 3f;            // 짧게 유지
    public float damage = 10f;

    [HideInInspector] public GameObject originPrefab;

    Rigidbody2D rb; float life;

    public void Init(string ownerTag, Vector2 direction, float bulletSpeed, float dmg, GameObject sourcePrefab)
    {
        owner = ownerTag;
        dir = direction.sqrMagnitude > 0 ? direction.normalized : Vector2.up;
        speed = bulletSpeed; damage = dmg; originPrefab = sourcePrefab; life = 0f;

        if (!rb) rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic; rb.gravityScale = 0f; rb.freezeRotation = true;
        rb.linearDamping = 0f; rb.angularDamping = 0f; rb.linearVelocity = dir * speed;

        var p = transform.position; transform.position = new Vector3(p.x, p.y, 0f);

        // 안전 타이머(풀 객체에 장착된 컴포넌트 사용)
        GetComponent<PoolAutoReturn>().Arm(originPrefab, lifeTime + 1.5f);

        CameraShake.Ensure();
        gameObject.SetActive(true);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var col = GetComponent<Collider2D>(); if (col) col.isTrigger = true;
        if (rb) { rb.gravityScale = 0f; rb.freezeRotation = true; }
    }

    void OnEnable() { life = 0f; }

    void Update()
    {
        life += Time.deltaTime;
        if (life >= lifeTime) { Despawn(); return; }

        // 물리 막혔을 때의 백업 전진
        if (!rb || rb.bodyType != RigidbodyType2D.Dynamic)
            transform.position += (Vector3)(dir * speed * Time.deltaTime);

        // 뷰포트 밖 즉시 회수
        var cam = Camera.main;
        if (cam)
        {
            var v = cam.WorldToViewportPoint(transform.position);
            if (v.z < 0f || v.x < -0.05f || v.x > 1.05f || v.y < -0.05f || v.y > 1.05f)
            { Despawn(); return; }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == "Player" && other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>()?.TakeDamage(damage);
            CameraShake.ShakeNow(0.18f, 0.23f);   // 확실히 보이게
            Despawn();
        }
        else if (owner == "Enemy" && other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeHit();
            CameraShake.ShakeNow(0.20f, 0.27f);
            Despawn();
        }
    }

    void OnBecameInvisible() => Despawn();

    void Despawn()
    {
        if (rb) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
        if (originPrefab) SimplePool.Return(originPrefab, gameObject);
        else Destroy(gameObject); // 최후 안전
    }
}