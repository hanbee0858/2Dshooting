using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public Transform player;          // 씬에서 할당하거나, 런타임에 찾기

    [Header("Bullet Params")]
    public float bulletSpeed = 8f;
    public float bulletLife = 2f;
    public float bulletAccel = 0f;

    [Header("Fire Control")]
    public float fireInterval = 1.2f;
    private float _cooldown;

    void Update()
    {
        _cooldown -= Time.deltaTime;
        if (_cooldown <= 0f)
        {
            TryShootAtPlayer();
            _cooldown = fireInterval;
        }
    }

    void TryShootAtPlayer()
    {
        if (player == null || bulletPrefab == null || firePoint == null) return;

        // 목표 방향 (★ 반드시 Vector2로)
        Vector2 dir = ((Vector2)(player.position - firePoint.position)).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = (Vector2)firePoint.up;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        BulletBase b = go.GetComponent<BulletBase>();
        if (b == null)
        {
            Debug.LogError("BulletBase 컴포넌트가 bulletPrefab에 없습니다!");
            return;
        }

        // 총알 회전은 선택 (화살표가 바라보게 하고 싶다면)
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0f, 0f, ang);

        // Init (형식/순서 정확!)
        b.Init(dir, bulletSpeed, bulletLife, transform, bulletAccel);
    }
}