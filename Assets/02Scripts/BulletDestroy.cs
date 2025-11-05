using UnityEngine;

public class BulletDestroy : MonoBehaviour
{
    void OnBecameInvisible()
    {
        Destroy(gameObject); // 화면 밖 나가면 자동 제거
    }
}
