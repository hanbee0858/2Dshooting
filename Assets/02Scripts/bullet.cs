using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("속도/가속 설정")]
    public float startSpeed = 4f;
    public float endSpeed = 12f;
    public float accelTime = 0.8f;

    [Header("수명")]
    public float lifeTime = 5f;

    [HideInInspector] public GameObject originPrefab;

    private Rigidbody2D rb;
    private Vector2 dir = Vector2.up;
    private float t, life;

    public void Init(Vector2 direction, GameObject prefabKey)
    {
        dir = direction.sqrMagnitude > 0 ? direction.normalized : Vector2.up;
        originPrefab = prefabKey;
        t = 0f;
        life = 0f;

        // SubBullet이면 속도 빠르게 세팅
        if (gameObject.name.Contains("Sub") || gameObject.name.Contains("sub"))
        {
            startSpeed = 7f;
            endSpeed = 18f;
            accelTime = 0.4f;
        }

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = Mathf.Max(sr.sortingOrder, 100);
            var c = sr.color; c.a = 1f; sr.color = c;
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        t = 0f;
        life = 0f;
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;
        life += Time.fixedDeltaTime;

        float k = Mathf.Clamp01(t / accelTime);
        float speed = Mathf.Lerp(startSpeed, endSpeed, k);
        rb.linearVelocity = dir * speed;

        if (life >= lifeTime)
            ReturnToPool();
    }

    void OnTriggerEnter2D(Collider2D other) => ReturnToPool();
    void OnBecameInvisible() => ReturnToPool();

    private void ReturnToPool()
    {
        if (!gameObject.activeSelf) return;
        rb.linearVelocity = Vector2.zero;

        if (originPrefab != null)
            SimplePool.Return(originPrefab, gameObject, null);
        else
            gameObject.SetActive(false);
    }
}