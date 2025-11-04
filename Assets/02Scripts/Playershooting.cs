using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("총알 프리팹 설정")]
    public GameObject mainBulletPrefab;
    public GameObject subBulletPrefab;

    [Header("발사 세팅")]
    public float fireCooldown = 0.6f;
    public bool autoFire = true;

    [Header("발사 위치 오프셋")]
    public float muzzleOffset = 0.6f;
    public float sideOffset = 0.3f;

    private float fireTimer;

    void Update()
    {
        fireTimer -= Time.deltaTime;

        // 모드 전환
        if (Input.GetKeyDown(KeyCode.Alpha1)) autoFire = true;
        if (Input.GetKeyDown(KeyCode.Alpha2)) autoFire = false;

        // 발사 조건
        if (autoFire)
        {
            if (fireTimer <= 0f)
            {
                FirePattern();
                fireTimer = fireCooldown;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && fireTimer <= 0f)
            {
                FirePattern();
                fireTimer = fireCooldown;
            }
        }
    }

    void FirePattern()
    {
        Vector2 forward = Vector2.up;

        Vector3 mainPos = transform.position + (Vector3)(forward * muzzleOffset);
        Vector3 leftPos = transform.position + new Vector3(-sideOffset, 0f, 0f) + (Vector3)(forward * muzzleOffset * 0.8f);
        Vector3 rightPos = transform.position + new Vector3(sideOffset, 0f, 0f) + (Vector3)(forward * muzzleOffset * 0.8f);

        // 메인탄
        SpawnBullet(mainBulletPrefab, mainPos, forward);

        // 좌우 보조탄
        SpawnBullet(subBulletPrefab, leftPos, forward);
        SpawnBullet(subBulletPrefab, rightPos, forward);
    }

    void SpawnBullet(GameObject prefab, Vector3 pos, Vector2 dir)
    {
        if (prefab == null)
        {
            Debug.LogError($"[PlayerShooting] {name}: Bullet Prefab이 비어 있습니다! 🔴");
            return;
        }

        GameObject go = SimplePool.Get(prefab, pos, Quaternion.identity);
        if (go == null)
        {
            Debug.LogError($"[PlayerShooting] {name}: {prefab.name} 풀 생성 실패 ⚠️");
            return;
        }

        Bullet b = go.GetComponent<Bullet>();
        if (b != null)
            b.Init(dir, prefab);
        else
            Debug.LogError($"[PlayerShooting] {prefab.name}에 Bullet 스크립트가 없습니다!");
    }
}