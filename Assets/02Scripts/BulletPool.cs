using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool I;
    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject Get(GameObject prefab, Transform parent = null)
    {
        if (!pools.TryGetValue(prefab, out var q)) { q = new Queue<GameObject>(); pools[prefab] = q; }

        GameObject go = q.Count > 0 ? q.Dequeue() : Instantiate(prefab);
        go.SetActive(true);
        if (parent) go.transform.SetParent(parent);
        return go;
    }

    public void Return(GameObject prefab, GameObject inst)
    {
        inst.SetActive(false);
        inst.transform.SetParent(transform);
        if (!pools.TryGetValue(prefab, out var q)) { q = new Queue<GameObject>(); pools[prefab] = q; }
        q.Enqueue(inst);
    }
}