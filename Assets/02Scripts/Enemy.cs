using UnityEngine;

// 이 스크립트를 가진 GameObject에는 Health 컴포넌트가 필수로 붙도록 합니다.
// Health 컴포넌트가 없으면 Unity 에디터에서 자동으로 추가됩니다.
[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
    [Header("폭발 설정")]
    public GameObject explosionPrefab; // 적이 파괴될 때 재생할 폭발 프리팹 (Inspector에서 할당)

    private Health enemyHealth; // 현재 적의 Health 컴포넌트 참조

    void Awake()
    {
        // GameObject에 붙어있는 Health 컴포넌트를 가져옵니다.
        enemyHealth = GetComponent<Health>();

        // Health 컴포넌트가 존재하는지 확인합니다.
        if (enemyHealth == null)
        {
            Debug.LogError("Error: 'Enemy' 스크립트에 'Health' 컴포넌트가 없습니다.", this);
            return;
        }

        // Health 컴포넌트의 OnDeath 이벤트에 HandleDeath 메서드를 구독합니다.
        // 적의 체력이 0이 되면 OnDeath 이벤트가 호출되고 HandleDeath 메서드가 실행됩니다.
        enemyHealth.OnDeath += HandleDeath;
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때, Health 컴포넌트의 OnDeath 이벤트 구독을 해지하여
        // NullReferenceException 오류를 방지합니다.
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= HandleDeath;
        }
    }

    /// <summary>
    /// 적의 체력이 0이 되었을 때 호출되는 메서드.
    /// 폭발 효과를 생성하고 적 GameObject를 파괴합니다.
    /// </summary>
    private void HandleDeath()
    {
        // 폭발 프리팹이 설정되어 있다면, 현재 적의 위치와 회전에서 폭발 효과를 생성합니다.
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning($"Warning: {gameObject.name}의 폭발 프리팹이 설정되지 않았습니다.", this);
        }

        // 현재 적 GameObject를 파괴합니다.
        Destroy(gameObject);
    }
}
