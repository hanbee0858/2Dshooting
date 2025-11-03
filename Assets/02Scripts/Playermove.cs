using UnityEngine;

using UnityEnusing UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("이동 속도")]
    public float speed = 5f;          // 기본 속도
    public float minSpeed = 1f;       // 최소 속도
    public float maxSpeed = 15f;      // 최대 속도
    public float speedStep = 1f;      // Q/E 속도 변경량
    public float boostMultiplier = 1.2f; // ✅ Shift 누를 때 곱해질 배수

    [Header("아랫부분 이동 비율 / 가장자리 여백")]
    [Range(0.05f, 0.8f)]
    public float bottomBandPercent = 0.33f; // 화면 하단 1/3만 이동 가능
    public float edgePadding = 0.2f;

    float minX, maxX, minY, maxY;
    float leftWrapX, rightWrapX;

    //  플레이어의 시작 위치 저장
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position; // ✅ 게임 시작 시 현재 위치를 저장

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

        //  Shift 누르면 임시 가속
        float currentSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            currentSpeed *= boostMultiplier;

        //  이동 입력 (즉시형)
        float h = 0f;
        float v = 0f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) h = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) v = 1f;
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) v = -1f;

        Vector3 dir = new Vector3(h, v, 0f).normalized;

        // 이동
        Vector3 pos = transform.position;
        pos += dir * currentSpeed * Time.deltaTime;

        //  세로 이동은 화면 하단 1/3만 허용
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        //  좌우 화면 워프
        if (pos.x < leftWrapX)
            pos.x = rightWrapX;
        else if (pos.x > rightWrapX)
            pos.x = leftWrapX;

        //  R키: 원점(초기 위치)으로 즉시 복귀
        if (Input.GetKeyDown(KeyCode.R))
            pos = startPosition;

        // 적용
        transform.position = pos;
    }
}