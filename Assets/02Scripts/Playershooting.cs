using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;          // 총알 나가는 위치/방향
    public GameObject bulletPrefab;      // BulletBase가 붙은 프리팹

    [Header("Bullet Params")]
    public float bulletSpeed = 12f;
    public float bulletLife = 2f;
    public float bulletAccel = 0f;       // 가속 필요 없으면 0

    void Update()
    {
        // 예시: 스페이스로 발사
        if (Input.GetKeyDown(KeyCode.Space))
            Fire();
    }

    public void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // 스폰
        GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // BulletBase 확보
        BulletBase b = go.GetComponent<BulletBase>();
        if (b == null)
        {
            Debug.LogError("BulletBase 컴포넌트가 bulletPrefab에 없습니다!");
            return;
        }

        // 방향: firePoint의 '위(up)'를 사용 (2D 기준으로 총구 방향)
        Vector2 dir = (Vector2)firePoint.up;              // ★ Vector2로!

        // Init 호출 (형식/순서 정확!)
        b.Init(dir, bulletSpeed, bulletLife, transform, bulletAccel);
    }
}