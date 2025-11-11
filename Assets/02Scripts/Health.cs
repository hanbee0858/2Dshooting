using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP = 100;

    public System.Action onDeath;

    void Awake() { currentHP = Mathf.Clamp(currentHP, 0, maxHP); }

    public void TakeDamage(int dmg)
    {
        currentHP -= Mathf.Max(0, dmg);
        if (currentHP <= 0)
        {
            onDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}
