using UnityEngine;

public class BulletSimple : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float life = 2f;
    public float accel = 0f;

    [Header("Damage")]
    public int damage = 15;          // 한 발 데미지
    public string ownerTag = "Player";    // "Player" or "Enemy"

    [Header("Safety")]
    public bool requireEnemyOwner = true; // 적탄은 주인(EnemyWander) 없으면 바로 삭제
    public bool verbose = false;

    // 전역 탄 카운트(적탄 한정)
    public static int GlobalEnemyBullets = 0;
    public static int GlobalEnemyBulletsMax = 24;

    Vector2 dir = Vector2.up;
    float t;
    bool counted = false;

    public void Init(Vector2 direction, float spd, float lf, float ac = 0f, int dmg = 15, string owner = "Player")
    {
        dir = direction.normalized;
        speed = spd;
        life = lf;
        accel = ac;
        damage = dmg;
        ownerTag = owner;
        t = 0f;

        // 적탄 전역 상한 체크
        if (ownerTag == "Enemy")
        {
            if (GlobalEnemyBullets >= GlobalEnemyBulletsMax)
            {
                if (verbose) Debug.LogWarning("[BulletSimple] 글로벌 적탄 한도 초과 → 스폰 취소", this);
                Destroy(gameObject);
                return;
            }
            GlobalEnemyBullets++;
            counted = true;
        }
    }

    void Awake()
    {
        // 적탄이면 주인 강제 확인
        if (ownerTag == "Enemy" && requireEnemyOwner)
        {
            var p = transform.parent;
            var ok = p && p.GetComponentInParent<EnemyWander>() != null;
            if (!ok)
            {
                if (verbose) Debug.LogWarning("[BulletSimple] 적탄인데 주인(EnemyWander)이 없음 → 자멸", this);
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t >= life) { Destroy(gameObject); return; }

        speed += accel * Time.deltaTime;
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    void OnBecameInvisible() { Destroy(gameObject); }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other || !other.gameObject.activeInHierarchy) return;

        // 아군 무시
        if (other.CompareTag(ownerTag)) return;

        // 상대 태그에만 데미지
        string targetTag = ownerTag == "Player" ? "Enemy" : "Player";
        if (other.CompareTag(targetTag))
        {
            var hp = other.GetComponentInParent<Health>();
            if (hp) hp.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (counted && ownerTag == "Enemy")
        {
            GlobalEnemyBullets = Mathf.Max(0, GlobalEnemyBullets - 1);
        }
    }
}