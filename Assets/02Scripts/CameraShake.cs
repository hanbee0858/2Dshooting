using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake I;
    public Transform shakeTarget;               // CameraRig
    Vector3 origin; float t, power;

    void Awake()
    {
        I = this;
        var cam = Camera.main ? Camera.main.transform : FindFirstObjectByType<Camera>()?.transform;
        if (!cam) return;

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

    void LateUpdate()
    {
        if (!shakeTarget) return;
        if (t > 0f)
        {
            shakeTarget.localPosition = origin + (Vector3)Random.insideUnitCircle * power;
            t -= Time.unscaledDeltaTime;
            if (t <= 0f) shakeTarget.localPosition = origin;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        if (!shakeTarget) return;
        t = Mathf.Max(0.01f, duration);
        power = Mathf.Max(0f, magnitude);
    }

    public static void ShakeNow(float d, float m)
    {
        var s = I ? I : (Camera.main ? Camera.main.GetComponent<CameraShake>() : null);
        if (!s && Camera.main) s = Camera.main.gameObject.AddComponent<CameraShake>();
        s?.Shake(d, m);
    }
}