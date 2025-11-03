using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("이동 속도")]
    public float speed = 5f;          // 현재 속도
    public float minSpeed = 1f;       // 최소 속도
    public float maxSpeed = 15f;      // 최대 속도
    public float speedStep = 1f;      // Q/E 한 번 누를 때 속도 증감량

    [Header("아랫부분 이동 비율 / 가장자리 여백")]
    [Range(0.05f, 0.8f)]
    public float bottomBandPercent = 0.33f; // 화면 세로의 아래쪽 1/3 구간
    public float edgePadding = 0.2f;

    // 이동 가능 경계
    float minX, maxX, minY, maxY;
    float leftWrapX, rightWrapX;

    void Start()
    {
        var cam = Camera.main;
        float distance = Mathf.Abs(transform.position.z - cam.transform.position.z);

        Vector3 worldBL = cam.ViewportToWorldPoint(new Vector3(0f, 0f, distance));
        Vector3 worldTR = cam.ViewportToWorldPoint(new Vector3(1f, 1f, distance));

        float worldMinX = worldBL.x + edgePadding;
        float worldMaxX = worldTR.x - edgePadding;
        float worldMinY = worldBL.y + edgePadding;
        float worldMaxY = worldTR.y - edgePadding;

        float totalHeight = worldMaxY - worldMinY;
        float bandHeight = totalHeight * bottomBandPercent;

        float halfW = 0f, halfH = 0f;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            halfW = sr.bounds.extents.x;
            halfH = sr.bounds.extents.y;
        }

        minX = worldMinX + halfW;
        maxX = worldMaxX - halfW;
        minY = worldMinY + halfH;
        maxY = worldMinY + bandHeight - halfH;

        leftWrapX = worldMinX - halfW;
        rightWrapX = worldMaxX + halfW;
    }

    void Update()
    {
        //  Q/E 키로 속도 조절
        if (Input.GetKeyDown(KeyCode.Q))
            speed = Mathf.Min(speed + speedStep, maxSpeed);
        if (Input.GetKeyDown(KeyCode.E))
            speed = Mathf.Max(speed - speedStep, minSpeed);

        //  가속도 없는 즉시형 입력 (한 번 누르면 바로 이동, 떼면 정지)
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            h = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            h = -1f;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            v = 1f;
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            v = -1f;

        // 입력 방향 (정규화로 대각선 속도 일정)
        Vector3 dir = new Vector3(h, v, 0f).normalized;

        // 즉시 위치 이동
        Vector3 pos = transform.position;
        pos += dir * speed * Time.deltaTime;

        // 세로 이동 제한 (화면 하단 1/3)
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // 좌우 랩
        if (pos.x < leftWrapX)
            pos.x = rightWrapX;
        else if (pos.x > rightWrapX)
            pos.x = leftWrapX;

        transform.position = pos;
    }
}