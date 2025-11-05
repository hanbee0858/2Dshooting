using UnityEngine;

public static class DevLog
{
    public static bool enabled = false; // 필요할 때만 true로

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Log(object msg) { if (enabled) Debug.Log(msg); }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Warn(object msg) { if (enabled) Debug.LogWarning(msg); }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Error(object msg) { if (enabled) Debug.LogError(msg); }

    // 런타임에서 콘솔 자체를 끄고 싶을 때(에디터에서도)
    public static void KillAllLogsAtRuntime()
    {
        Debug.unityLogger.logEnabled = false;
    }
}
