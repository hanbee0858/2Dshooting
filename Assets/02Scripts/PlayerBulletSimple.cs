using UnityEngine;

public class PlayerBulletSimple : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float life = 2f;
    public float accel = 0f;

    [Header("Damage")]
    public int damage = 15;
    public string ownerTag = "Player";

    Vector2 dir = Vector2.up;
    float t;

    public void Init(Vector2 direction, float spd, float lf, float ac = 0f, int dmg = 15, string owner = "Player")
    {
        dir = direction.normalized;
        speed = spd; life = lf; accel = ac; damage = dmg;
        ownerTag = owner;
        t = 0f;
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t >= life) { Destroy(gameObject); return; }

        speed += accel * Time.deltaTime;
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    void OnBecameInvisible() => Destroy(gameObject);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other || !other.gameObject.activeInHierarchy) return;
        if (other.CompareTag(ownerTag)) return;   // 아군 무시
        if (other.CompareTag("Enemy"))
        {
            var hp = other.GetComponentInParent<Health>();
            if (hp) hp.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}