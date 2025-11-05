using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("프리팹")]
    public GameObject mainBulletPrefab;
    public GameObject subBulletPrefab;

    [Header("발사 지점")]
    public Transform firePoint;

    [Header("데미지/속도")]
    public float mainBulletDamage = 80f;  // ✅ 주탄 80
    public float subBulletDamage = 30f;  // ✅ 보탄 30
    public float mainBulletSpeed = 12f;
    public float subBulletSpeed = 9f;

    [Header("발사 설정")]
    public bool isAuto = false;          // 기본 수동(스페이스)
    public float fireInterval = 0.45f;    // "간격" 개념

    [Header("동시 활성 제한")]
    public int maxActiveMain = 120;
    public int maxActiveSub = 80;

    [Header("키")]
    public KeyCode manualFireKey = KeyCode.Space;
    public KeyCode forceFireKey = KeyCode.Q;

    // 누적 타이머
    float fireAccum = 0f;
    bool wantFire = false;

    void Awake()
    {
        if (!firePoint)
        {
            var t = transform.Find("FirePoint");
            if (t) firePoint = t;
            else
            {
                var go = new GameObject("FirePoint");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                firePoint = go.transform;
            }
        }
        if (!mainBulletPrefab) mainBulletPrefab = Resources.Load<GameObject>("MainBullet");
        if (!subBulletPrefab) subBulletPrefab = Resources.Load<GameObject>("SubBullet");
        CameraShake.Ensure();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) isAuto = true;
        if (Input.GetKeyDown(KeyCode.Alpha2)) isAuto = false;

        // 입력 의지
        if (Input.GetKeyDown(forceFireKey)) wantFire = true;
        if (!isAuto) wantFire |= Input.GetKey(manualFireKey);
        else wantFire = true;

        // 누적 타이머로 정확 간격
        fireAccum += Time.deltaTime;
        while (wantFire && fireAccum >= fireInterval)
        {
            FireTripleOnce();           // 한 프레임에 주+좌+우 동시
            fireAccum -= fireInterval;
        }
        wantFire = false;
    }

    void FireTripleOnce()
    {
        if (!firePoint || !mainBulletPrefab) return;

        // 보조탄 2발이 "같은 프레임"에 나갈 수 없으면 생략(엇박 방지)
        int needSubs = subBulletPrefab ? 2 : 0;
        bool canMain = SimplePool.ActiveCount(mainBulletPrefab) < maxActiveMain;
        bool canSubs = (needSubs == 0) ||
                       (SimplePool.ActiveCount(subBulletPrefab) + needSubs <= maxActiveSub);
        if (!canMain) return;

        // 주탄
        var m = SimplePool.Get(mainBulletPrefab, firePoint.position, Quaternion.identity);
        m.GetComponent<BulletBase>()?.Init("Player", Vector2.up, mainBulletSpeed, mainBulletDamage, mainBulletPrefab);

        // 보조탄(둘 다 동시)
        if (needSubs > 0 && canSubs)
        {
            var L = firePoint.position + new Vector3(-0.3f, 0f, 0f);
            var R = firePoint.position + new Vector3(0.3f, 0f, 0f);

            var s1 = SimplePool.Get(subBulletPrefab, L, Quaternion.identity);
            s1.GetComponent<BulletBase>()?.Init("Player", new Vector2(-0.12f, 1f), subBulletSpeed, subBulletDamage, subBulletPrefab);

            var s2 = SimplePool.Get(subBulletPrefab, R, Quaternion.identity);
            s2.GetComponent<BulletBase>()?.Init("Player", new Vector2(0.12f, 1f), subBulletSpeed, subBulletDamage, subBulletPrefab);
        }
    }
}