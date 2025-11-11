using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 필요

/// <summary>
/// 플레이어 사격(수동/자동). FirePoint 자동 복구, 프리팹 자동 로드, 누락 시 런타임 보정 포함.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Refs")]
    public Transform FirePoint;                     // Player의 자식. 없으면 자동 생성.
    public GameObject BulletPrefab;                 // Resources/PlayerBullet(또는 PlayerBulletSimple).
    public string BulletResourcePath = "PlayerBullet";

    [Header("Control")]
    public KeyCode FireKey = KeyCode.Space;
    public KeyCode AutoOnKey = KeyCode.Alpha1;
    public KeyCode AutoOffKey = KeyCode.Alpha2;

    [Header("Bullet")]
    public float BulletSpeed = 6f;
    public float BulletLife = 2f;
    public float BulletAccel = 0f;
    public float FireInterval = 0.15f; // 기본 발사 간격

    [Header("State")]
    public bool AutoBattleEnabled = false;

    // 난사 속도 증가 아이템 관련 변수
    private float _originalFireInterval; // 원래 발사 간격 저장
    private Coroutine _rapidFireCoroutine; // 난사 효과 코루틴 참조

    private float _cd;

    private void Awake()
    {
        // FirePoint 자동 복구.
        if (FirePoint == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Replace(" ", "").Equals("FirePoint", System.StringComparison.OrdinalIgnoreCase))
                {
                    FirePoint = t;
                    break;
                }
            }

            if (FirePoint == null)
            {
                var go = new GameObject("FirePoint");
                FirePoint = go.transform;
                FirePoint.SetParent(transform);
                FirePoint.localPosition = new Vector3(0f, 0.5f, 0f);
                FirePoint.localRotation = Quaternion.identity;
            }
        }

        // 프리팹 자동 로드(비어 있을 때).
        if (BulletPrefab == null && !string.IsNullOrEmpty(BulletResourcePath))
        {
            var res = Resources.Load<GameObject>(BulletResourcePath);
            if (res != null)
            {
                BulletPrefab = res;
            }
        }

        // 원래 발사 간격 저장
        _originalFireInterval = FireInterval;
    }

    private void Update()
    {
        if (Input.GetKeyDown(AutoOnKey))
        {
            AutoBattleEnabled = true;
        }

        if (Input.GetKeyDown(AutoOffKey))
        {
            AutoBattleEnabled = false;
        }

        _cd -= Time.deltaTime;

        if ((Input.GetKey(FireKey) || AutoBattleEnabled) && _cd <= 0f)
        {
            if (FirePoint == null || BulletPrefab == null)
            {
                return;
            }

            Fire();
            _cd = FireInterval;
        }
    }

    /// <summary>
    /// 난사 속도 증가 효과를 적용합니다.
    /// </summary>
    /// <param name="multiplier">발사 간격에 곱할 비율 (예: 0.5면 2배 빨라짐)</param>
    /// <param name="duration">효과 지속 시간</param>
    public void ApplyRapidFire(float multiplier, float duration)
    {
        // 이미 난사 효과 코루틴이 실행 중이라면 중지하고 새로 시작
        if (_rapidFireCoroutine != null)
        {
            StopCoroutine(_rapidFireCoroutine);
        }
        _rapidFireCoroutine = StartCoroutine(RapidFireEffectCoroutine(multiplier, duration));
    }

    /// <summary>
    /// 난사 속도 증가 효과를 관리하는 코루틴.
    /// </summary>
    private IEnumerator RapidFireEffectCoroutine(float multiplier, float duration)
    {
        // 현재 발사 간격을 원래 값으로 되돌립니다 (중첩 방지/초기화).
        FireInterval = _originalFireInterval;

        // 발사 간격 적용 (곱하기)
        FireInterval *= multiplier;

        Debug.Log($"난사 효과 적용! 현재 발사 간격: {FireInterval}", this);

        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 효과 종료 후 원래 발사 간격으로 되돌림
        FireInterval = _originalFireInterval;
        Debug.Log($"난사 효과 종료! 원래 발사 간격으로 복귀: {FireInterval}", this);

        _rapidFireCoroutine = null; // 코루틴이 끝났음을 표시
    }

    /// <summary>
    /// 한 발 발사(누락 시 런타임 보정 포함).
    /// </summary>
    public void Fire()
    {
        Vector2 dir = FirePoint.up;

        var go = Instantiate(BulletPrefab, FirePoint.position, Quaternion.identity);
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        go.transform.rotation = Quaternion.Euler(0, 0, ang);

        // 방탄: BulletSimple 없으면 자동 추가 + 기본 물리 세팅.
        var b = go.GetComponent<BulletSimple>();
        if (b == null)
        {
            b = go.AddComponent<BulletSimple>();

            var rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }

            var col = go.GetComponent<Collider2D>();
            if (col == null)
            {
                var c = go.AddComponent<CircleCollider2D>();
                c.radius = 0.08f;
                c.isTrigger = true;
            }
            else
            {
                col.isTrigger = true;
            }
        }

        // 플레이어 탄 초기화.
        b.RequireEnemyOwner = false;
        b.Init(dir, BulletSpeed, BulletLife, BulletAccel, 15, "Player");
    }
}                                                           