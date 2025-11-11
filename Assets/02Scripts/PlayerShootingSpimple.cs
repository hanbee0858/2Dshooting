using UnityEngine;

public class PlayerShootingSimple : MonoBehaviour
{
    [Header("Refs")]
    public Transform firePoint;
    public GameObject bulletPrefab;                 // Resources/PlayerBulletSimple
    public string bulletResourcePath = "PlayerBulletSimple";

    [Header("Control")]
    public KeyCode fireKey = KeyCode.Space;
    public KeyCode autoOnKey = KeyCode.Alpha1;
    public KeyCode autoOffKey = KeyCode.Alpha2;

    [Header("Bullet")]
    public float bulletSpeed = 6f;
    public float bulletLife = 2f;
    public float bulletAccel = 0f;
    public float fireInterval = 0.15f;

    public bool autoBattleEnabled = false;
    float cd;

    void Awake()
    {
        if (!firePoint) Debug.LogError("[PSS] firePoint NULL", this);
        if (!bulletPrefab)
        {
            var res = Resources.Load<GameObject>(bulletResourcePath);
            if (res) bulletPrefab = res; else Debug.LogError($"[PSS] bulletPrefab NULL: Resources/{bulletResourcePath}", this);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(autoOnKey)) autoBattleEnabled = true;
        if (Input.GetKeyDown(autoOffKey)) autoBattleEnabled = false;

        cd -= Time.deltaTime;
        if ((Input.GetKey(fireKey) || autoBattleEnabled) && cd <= 0f)
        {
            if (!firePoint || !bulletPrefab) return;
            Fire();
            cd = fireInterval;
        }
    }

    void Fire()
    {
        Vector2 dir = firePoint.up;
        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0, 0, ang);

        var b = go.GetComponent<BulletSimple>();
        if (!b) { Debug.LogError("[PSS] BulletSimple 누락", go); return; }

        b.Init(dir, bulletSpeed, bulletLife, bulletAccel, 15, "Player");
    }
}
