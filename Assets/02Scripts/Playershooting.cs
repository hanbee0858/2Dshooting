using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("총알")]
    public GameObject bulletPrefab;      // Rigidbody2D + Collider2D + Bullet.cs
    public float fireDelay = 0.12f;
    public float muzzleOffset = 0.6f;    // 플레이어 '앞'(위)으로 띄워서 스폰

    Collider2D[] playerCols;
    float lastFireTime;

    void Awake()
    {
        // 플레이어 및 자식의 2D 콜라이더를 모아, 총알과 충돌을 즉시 무시합니다.
        playerCols = GetComponentsInChildren<Collider2D>(includeInactive: false);

        // 탑다운 기본 세팅 보강(혹시 빠졌을 경우)
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= lastFireTime + fireDelay)
        {
            FireUp(); // 항상 '위'로 발사
            lastFireTime = Time.time;
        }
    }

    void FireUp()
    {
        if (!bulletPrefab) return;

        Vector2 shootDir = Vector2.up; // ✅ 위로 고정
        Vector3 spawnPos = transform.position + (Vector3)(shootDir * muzzleOffset);

        GameObject go = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // 스폰 즉시 플레이어와 충돌 무시(밀림/튐 방지)
        var bulletCol = go.GetComponent<Collider2D>();
        if (bulletCol != null && playerCols != null)
        {
            foreach (var pc in playerCols)
                if (pc != null) Physics2D.IgnoreCollision(bulletCol, pc, true);
        }

        // Bullet 컴포넌트로 방향 전달 (없으면 우회 처리)
        var bullet = go.GetComponent<Bullet>();
        if (bullet != null) bullet.Init(shootDir);
        else
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.gravityScale = 0f; rb.linearVelocity = shootDir * 8f; }
        }
    }
}