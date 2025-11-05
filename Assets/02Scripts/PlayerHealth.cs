using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHits = 3;
    int currentHits = 0;

    public void TakeHit()
    {
        currentHits++;
        Debug.Log($"[Player] 피격 {currentHits}/{maxHits}");

        if (CameraShake.I) CameraShake.I.Shake(0.2f, 0.2f);

        if (currentHits >= maxHits)
        {
            Debug.Log("[Player] 파괴됨!");
            Destroy(gameObject);
        }
    }
}
