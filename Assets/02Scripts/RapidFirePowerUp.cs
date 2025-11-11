using UnityEngine;

/// <summary>
/// 플레이어의 총알 난사 속도를 일시적으로 증가시키는 아이템.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))] // 충돌 감지를 위해 Collider2D 필요
[RequireComponent(typeof(Rigidbody2D))]    // 물리적 상호작용을 위해 Rigidbody2D 필요
public class RapidFirePowerUp : MonoBehaviour
{
    [Header("효과 설정")]
    public float fireRateMultiplier = 0.5f; // 원래 발사 간격에 곱할 값 (0.5는 2배 빨라짐)
    public float effectDuration = 5f;       // 효과 지속 시간 (초)

    [Header("물리 설정")]
    public float floatSpeed = 0.5f;         // 아이템이 떠다니는 속도
    public float floatHeight = 0.1f;        // 아이템이 떠다니는 높이
    private Vector3 _startPosition;
    private float _timeOffset;

    void Awake()
    {
        // Collider2D를 트리거로 설정 (물리적 충돌이 아닌 감지 모드)
        var collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            // Collider2D의 반경을 아이템 크기에 맞게 조절할 수 있습니다 (예: 0.5f)
            collider.radius = 0.25f; 
        }

        // Rigidbody2D 설정 (중력 영향 없이 고정)
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // 물리 영향X, 스크립트로 제어
            rb.gravityScale = 0f;                    // 중력 영향X
        }

        _startPosition = transform.position;
        _timeOffset = Random.Range(0f, 100f); // 각 아이템이 다른 타이밍으로 떠다니도록
    }

    void Update()
    {
        // 아이템이 위아래로 살짝 떠다니는 효과
        float newY = _startPosition.y + Mathf.Sin((Time.time + _timeOffset) * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }


    /// <summary>
    /// 다른 콜라이더와 충돌했을 때 호출됩니다.
    /// </summary>
    /// <param name="other">충돌한 Collider2D</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 태그를 가진 오브젝트와 충돌했는지 확인
        // 플레이어 오브젝트에 "Player" 태그를 할당해야 합니다.
        if (other.CompareTag("Player"))
        {
            // 플레이어의 PlayerShooting 스크립트를 찾습니다.
            PlayerShooting playerShooting = other.GetComponent<PlayerShooting>();

            if (playerShooting != null)
            {
                // PlayerShooting 스크립트의 난사 속도 증가 메서드를 호출합니다.
                playerShooting.ApplyRapidFire(fireRateMultiplier, effectDuration);
                Debug.Log($"플레이어가 난사 속도 증가 아이템을 획득했습니다! {effectDuration}초 동안 발사 속도 {fireRateMultiplier}배 적용.", this);
            }
            
            // 아이템을 획득했으므로 파괴합니다.
            Destroy(gameObject);
        }
    }
}