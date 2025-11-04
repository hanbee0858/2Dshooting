using UnityEngine;

[DisallowMultipleComponent]
public class PlayerPhysicsFix : MonoBehaviour
{
    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;                 // 탑다운이면 중력 0
            rb.freezeRotation = true;             // 회전 고정
            rb.linearVelocity = Vector2.zero;           // 혹시 남아있던 속도 초기화
        }
    }
}
