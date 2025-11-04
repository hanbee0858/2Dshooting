using System.Collections.Generic;
using UnityEngine;

//  오브젝트 풀링 관리 전용 클래스 (static)
public static class SimplePool
{
    // 각 프리팹별로 큐(Queue) 관리
    private static readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    //  오브젝트 꺼내기
    public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!pools.TryGetValue(prefab, out var q))
        {
            q = new Queue<GameObject>();
            pools[prefab] = q;
        }

        GameObject go = q.Count > 0 ? q.Dequeue() : Object.Instantiate(prefab);
        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);
        return go;
    }

    //  오브젝트 반납하기
    public static void Return(GameObject prefab, GameObject instance, Transform parent = null)
    {
        if (prefab == null || instance == null) return;

        if (!pools.TryGetValue(prefab, out var q))
        {
            q = new Queue<GameObject>();
            pools[prefab] = q;
        }

        instance.SetActive(false);
        if (parent != null)
            instance.transform.SetParent(parent);
        q.Enqueue(instance);
    }
}
