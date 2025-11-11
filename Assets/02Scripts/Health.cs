using UnityEngine;
using System; // Action 델리게이트를 사용하기 위해 추가

public class Health : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 100;     // 최대 체력
    public int currentHealth;       // 현재 체력

    // 체력이 0이 되었을 때 호출될 이벤트
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth; // 시작 시 현재 체력을 최대 체력으로 설정
    }

    /// <summary>
    /// 피해를 입히는 메서드.
    /// </summary>
    /// <param name="amount">입을 피해량</param>
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return; // 이미 죽었다면 더 이상 피해를 입지 않음

        currentHealth -= amount; // 체력 감소
        Debug.Log($"{gameObject.name} 피해를 입었습니다. 현재 체력: {currentHealth}", this);

        if (currentHealth <= 0)
        {
            Die(); // 체력이 0 이하가 되면 죽음 처리 메서드 호출
        }
    }

    /// <summary>
    /// 체력을 회복하는 메서드.
    /// </summary>
    /// <param name="amount">회복할 체력량</param>
    public void Heal(int amount)
    {
        currentHealth += amount; // 체력 증가
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; // 최대 체력을 초과하지 않도록 제한
        }
        Debug.Log($"{gameObject.name} 체력을 회복했습니다. 현재 체력: {currentHealth}", this);
    }

    /// <summary>
    /// 죽음 처리 메서드. OnDeath 이벤트를 호출합니다.
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name}이(가) 파괴되었습니다!", this);
        OnDeath?.Invoke(); // 구독된 모든 메서드를 호출합니다.
                           // 이벤트를 호출한 후에는 GameObject가 파괴될 수 있으므로,
                           // 이 스크립트에서는 GameObject를 직접 파괴하지 않습니다.
                           // GameObject 파괴는 이벤트를 구독한 Enemy 스크립트에서 처리합니다.
    }
}
