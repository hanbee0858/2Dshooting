using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 공용 탄환: 이동/수명/충돌/데미지 + (적탄) 전역 개수 제한/고스트 가드.
/// </summary>
public class BulletSimple : MonoBehaviour
{
    [Header("Movement")]
    [FormerlySerializedAs("speed")] public float Speed = 6f;
    [FormerlySerializedAs("life")] public float Life = 2f;
    [FormerlySerializedAs("accel")] public float Accel = 0f;

    [Header("Damage")]
    [FormerlySerializedAs("damage")] public int Damage = 15;
    /// <summary> "Player" 또는 "Enemy" </summary>
    [FormerlySerializedAs("ownerTag")] public string OwnerTag = "Player";

    [Header("Safety")]
    /// <summary> 적탄은 반드시 EnemyWander 하위에서만 살도록 강제. </summary>
    public bool RequireEnemyOwner = true;

    /// <summary> 전역 적탄 개수(적탄만 카운트). </summary>
    public static int GlobalEnemyBullets = 0;

    /// <summary> 전역 적탄 최대 허용치. </summary>
    public static int GlobalEnemyBulletsMax = 24;

    private Vector2 _dir = Vector2.up;
    private float _t;
    private bool _counted;

    /// <summary>발사 시 초기화.</summary>
    public void Init(Vector2 direction, float speed, float life, float accel = 0f, int damage = 15, string owner = "Player")
    {
        _dir = direction.normalized;
        Speed = speed;
        Life = life;
        Accel = accel;
        Damage = damage;
        OwnerTag = owner;
        _t = 0f;

        if (OwnerTag == "Enemy")
        {
            if (GlobalEnemyBullets >= GlobalEnemyBulletsMax)
            {
                Destroy(gameObject);
                return;
            }

            GlobalEnemyBullets++;
            _counted = true;
        }
    }

    private void Awake()
    {
        if (OwnerTag == "Enemy" && RequireEnemyOwner)
        {
            var p = transform.parent;
            bool ok = p && p.GetComponentInParent<EnemyWander>() != null;
            if (!ok)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Update()
    {
        _t += Time.deltaTime;
        if (_t >= Life)
        {
            Destroy(gameObject);
            return;
        }

        Speed += Accel * Time.deltaTime;
        transform.position += (Vector3)(_dir * Speed * Time.deltaTime);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other || !other.gameObject.activeInHierarchy)
        {
            return;
        }

        if (other.CompareTag(OwnerTag))
        {
            return; // 아군 무시
        }

        string targetTag = OwnerTag == "Player" ? "Enemy" : "Player";
        if (other.CompareTag(targetTag))
        {
            var hp = other.GetComponentInParent<Health>();
            if (hp != null)
            {
                hp.TakeDamage(Damage);
            }

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_counted && OwnerTag == "Enemy")
        {
            GlobalEnemyBullets = Mathf.Max(0, GlobalEnemyBullets - 1);
        }
    }
}