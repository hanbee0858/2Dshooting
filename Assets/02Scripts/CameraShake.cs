using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake I;

    [Header("진동 대상(자동)")] public Transform shakeTarget;
    Vector3 origin;
    float t, power;

    void Awake()
    {
        I = this;
        // 메인 카메라 확보
        var cam = Camera.main ? Camera.main.transform : FindFirstObjectByType<Camera>()?.transform;
        if (!cam) return;

        // CameraRig(부모) 자동 생성 → 부모를 흔듭니다
        if (cam.parent == null || cam.parent.name != "CameraRig")
        {
            var rig = new GameObject("CameraRig").transform;
            rig.position = cam.position; rig.rotation = cam.rotation;
            cam.SetParent(rig, true);
            shakeTarget = rig;
        }
        else shakeTarget = cam.parent;

        origin = shakeTarget.localPosition;
    }

    void OnEnable() { if (shakeTarget) origin = shakeTarget.localPosition; }
    void OnDisable() { if (shakeTarget) shakeTarget.localPosition = origin; }

    public void Shake(float duration, float magnitude)
    {
        if (!shakeTarget) return;
        t = Mathf.Max(0.01f, duration);
        power = Mathf.Max(0f, magnitude);
    }

    void LateUpdate()
    {
        if (!shakeTarget) return;
        if (t > 0f)
        {
            // TimeScale과 무관하게 보이도록 unscaledDeltaTime 사용
            shakeTarget.localPosition = origin + (Vector3)Random.insideUnitCircle * power;
            t -= Time.unscaledDeltaTime;
            if (t <= 0f) shakeTarget.localPosition = origin;
        }
    }

    public static CameraShake Ensure()
    {
        if (I) return I;
        var cam = Camera.main ? Camera.main.gameObject : FindFirstObjectByType<Camera>()?.gameObject;
        if (!cam) return null;
        I = cam.GetComponent<CameraShake>(); if (!I) I = cam.AddComponent<CameraShake>();
        return I;
    }
    public static void ShakeNow(float d, float m) { var s = Ensure(); if (s) s.Shake(d, m); }
}
