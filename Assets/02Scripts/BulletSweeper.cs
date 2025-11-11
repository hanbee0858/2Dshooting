using UnityEngine;

/// <summary>
/// 경계/벽 트리거에서 탄환 정리. (태그: "Boundary","Limit","Wall")
/// </summary>
public class BulletSweeper : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other) return;

        if (other.CompareTag("Boundary") || other.CompareTag("Limit") || other.CompareTag("Wall"))
        {
            var bullet = other.GetComponent<BulletSimple>();
            if (bullet != null)
            {
                Destroy(other.gameObject);
            }
        }
    }
}
