using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    static readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
    static readonly Dictionary<GameObject, int> activeCount = new();

    public static int ActiveCount(GameObject prefab)
        => prefab != null && activeCount.TryGetValue(prefab, out var c) ? c : 0;

    public static GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!prefab) return null;

        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();
        if (!activeCount.ContainsKey(prefab)) activeCount[prefab] = 0;

        GameObject obj = pools[prefab].Count > 0 ? pools[prefab].Dequeue() : Object.Instantiate(prefab);
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);

        if (obj.TryGetComponent<Rigidbody2D>(out var rb))
        { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }

        activeCount[prefab]++;

        // 혹시 모를 누적 방지용(안전 타이머): 10초 뒤 자동 회수
        obj.GetComponent<PoolAutoReturn>()?.Arm(prefab, 10f);

        return obj;
    }

    public static void Return(GameObject prefab, GameObject obj)
    {
        if (!prefab || !obj) return;

        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();
        if (!activeCount.ContainsKey(prefab)) activeCount[prefab] = 0;

        if (obj.TryGetComponent<Rigidbody2D>(out var rb))
        { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }

        obj.SetActive(false);
        pools[prefab].Enqueue(obj);
        activeCount[prefab] = Mathf.Max(0, activeCount[prefab] - 1);
    }
}

// 🔒 풀 객체 안전타이머(있으면 사용)
public class PoolAutoReturn : MonoBehaviour
{
    GameObject key; float t; bool armed;
    public void Arm(GameObject prefabKey, float seconds) { key = prefabKey; t = seconds; armed = true; }
    void Update()
    {
        if (!armed) return;
        t -= Time.deltaTime;
        if (t <= 0f) { armed = false; SimplePool.Return(key, gameObject); }
    }
}
