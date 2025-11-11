using UnityEngine;

public class BulletSweeper : MonoBehaviour
{
    [Header("청소 주기(초)")]
    public float interval = 1.0f;

    [Header("디버그 로그")]
    public bool verbose = false;

    float cd;

    void Update()
    {
        cd -= Time.deltaTime;
        if (cd > 0f) return;
        cd = interval;

        var bullets = FindObjectsByType<BulletSimple>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int cleaned = 0;

        foreach (var b in bullets)
        {
            if (!b || !b.gameObject.activeInHierarchy) continue;

            // 적탄인데 주인(EnemyWander)이 없거나, parent가 전혀 없으면 → 삭제
            if (b.ownerTag == "Enemy")
            {
                var p = b.transform.parent;
                bool ok = p && p.GetComponentInParent<EnemyWander>() != null;
                if (!ok)
                {
                    Destroy(b.gameObject);
                    cleaned++;
                }
            }
        }

        if (verbose && cleaned > 0)
            Debug.Log($"[BulletSweeper] cleaned {cleaned} ghost enemy bullets", this);
    }
}
