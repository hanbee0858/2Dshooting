using UnityEngine;

public class GameBoot : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ForceResume()
    {
        Time.timeScale = 1f;   // 일시정지 걸려있던 프로젝트를 강제로 재생 상태로
    }
}
