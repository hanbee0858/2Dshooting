using UnityEngine;

public class ReplayUI : MonoBehaviour
{
    [Tooltip("리플레이를 제어할 대상(플레이어)")]
    public PlayerReplayController target;

    // 버튼 OnClick에 연결하세요.
    public void OnClickReplay()
    {
        if (!target) return;

        if (target.IsReplaying)
            target.StopReplay();
        else
            target.StartReplay();
    }

    // (선택) 기록 초기화 버튼에 연결
    public void OnClickClear()
    {
        if (!target) return;
        target.ClearRecording();
    }
}
