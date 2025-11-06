using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;           // 비워두면 Start에서 태그로 자동탐색
    public Transform firePoint;
    public GameObject bulletPrefab;    // BulletBase 포함 프리팹

    [Header("Move")]
    public float moveSpeed = 2f;
    public float stopDistance = 3f;    // 이 거리 이내면 멈추고 사격

    [Header("Shoot")]
    public float bulletSpeed = 8f;
    public float bulletLife = 2f;
    public float bulletAccel = 0f;
    public float fireInterval = 1.2f;  // 발사 주기
    float fireCd;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        // 타임스케일 0이면 아무 것도 안 함
        if (Time.timeScale <= 0f) return;

        // 이동
        if (player != null)
        {
            Vector2 toPlayer = (player.position - transform.position);
            float dist = toPlayer.magnitude;

            if (dist > stopDistance)
            {
                Vector2 dir = toPlayer.normalized;
                transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
            }
        }

        // 사격
        fireCd -= Time.deltaTime;
        if (fireCd <= 0f)
        {
            TryShoot();
            fireCd = fireInterval;
        }
    }

    void TryShoot()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;

        Vector2 dir = ((Vector2)(player.position - firePoint.position)).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = (Vector2)firePoint.up;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // 총알 회전(위=진행방향)
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, ang);

        var b = go.GetComponent<BulletBase>();
        if (b != null) b.Init(dir, bulletSpeed, bulletLife, transform, bulletAccel);
    }
}