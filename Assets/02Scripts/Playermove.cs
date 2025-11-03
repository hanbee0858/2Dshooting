using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float speed = 5f; //  이동 속도 조절 변수

    private void Update()
    {
        // 1️ 키보드 입력 감지
        float h = Input.GetAxis("Horizontal"); // ←, → 방향키
        float v = Input.GetAxis("Vertical");   // ↑, ↓ 방향키

        Debug.Log($"h: {h}, v: {v}");

        // 2️ 입력으로부터 방향 벡터 계산
        Vector2 direction = new Vector2(h, v);
        Debug.Log($"direction: {direction.x}, {direction.y}");

        // 3 현재 위치 가져오기
        Vector2 position = transform.position;

        // 4️새로운 위치 계산
        Vector2 newPosition = position + direction * speed * Time.deltaTime;

        // 5 새로운 위치 적용
        transform.position = newPosition;

        //  Time.deltaTime: 한 프레임당 흐른 시간 (속도 보정)
        // 이동속도 10
        //컴퓨터1 : 
        //컴퓨터2 :
    }
}
