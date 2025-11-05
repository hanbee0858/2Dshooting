using System.Collections.Generic;
using UnityEngine;

public static class SimplePool
{
    static readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
    static readonly Dictionary<GameObject, int> active = new();
    static Transform container;

    static Transform Container
    {
        get
        {
            if (!container)
            {
                var go = GameObject.Find("Bullets_Container");
                if (!go) go = new GameObject("Bullets_Container");
                container = go.transform;
            }
            return container;
        }
    }

    public static int ActiveCount(GameObject prefab)
        => prefab && active.TryGetValue(prefab, out var c) ? c : 0;

    public static GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!prefab) return null;
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();
        if (!active.ContainsKey(prefab)) active[prefab] = 0;

        GameObject obj = pools[prefab].Count > 0 ? pools[prefab].Dequeue() : Object.Instantiate(prefab);
        obj.transform.SetParent(Container, false);
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);

        // ✔ 풀키 자동 주입
        var key = obj.GetComponent<PoolKey>();
        if (!key) key = obj.AddComponent<PoolKey>();
        key.prefab = prefab;

        if (obj.TryGetComponent<Rigidbody2D>(out var rb)) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }

        active[prefab]++;
        return obj;
    }

    public static void Return(GameObject prefab, GameObject obj)
    {
        if (!prefab || !obj) return;
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();
        if (!active.ContainsKey(prefab)) active[prefab] = 0;

        if (obj.TryGetComponent<Rigidbody2D>(out var rb)) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }

        obj.SetActive(false);
        obj.transform.SetParent(Container, false);
        pools[prefab].Enqueue(obj);
        active[prefab] = Mathf.Max(0, active[prefab] - 1);
    }
}

// ✔ 오브젝트가 스스로 자신의 '원본 프리팹'을 알 수 있게 해주는 표식
public class PoolKey : MonoBehaviour
{
    public GameObject prefab;
}
