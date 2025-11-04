using System.Collections.Generic;
using UnityEngine;

public class BulletPooler : MonoBehaviour
{
    public static BulletPooler I;

    [Header("풀 세팅")]
    public GameObject bulletPrefab;   // 비어 있어도 자동으로 Resources에서 로드함
    public int initialSize = 60;
    public Transform container;

    readonly Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        I = this;

        // ✅ Inspector가 비어 있으면 Resources/Bullet.prefab 자동 로드
        if (bulletPrefab == null)
            bulletPrefab = Resources.Load<GameObject>("Bullet"); // Assets/Resources/Bullet.prefab

        if (bulletPrefab == null)
        {
            Debug.LogError("[BulletPooler] bulletPrefab이 없습니다. (Inspector에 넣거나 Resources/Bullet.prefab을 준비하세요)");
            enabled = false;
            return;
        }

        if (container == null)
        {
            var go = new GameObject("Bullets_Container");
            container = go.transform;
        }

        for (int i = 0; i < initialSize; i++)
        {
            var b = Instantiate(bulletPrefab, container);
            b.SetActive(false);
            pool.Enqueue(b);
        }
    }

    public GameObject Get(Vector3 position)
    {
        var go = pool.Count > 0 ? pool.Dequeue() : Instantiate(bulletPrefab, container);
        go.transform.SetPositionAndRotation(position, Quaternion.identity);
        go.SetActive(true);
        return go;
    }

    public void Return(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(container);
        pool.Enqueue(go);
    }
}