using UnityEngine;
// 네트워크 프레임워크에 따라 필요한 네임스페이스를 추가합니다.
// 예: using Unity.Netcode; // Unity Netcode for GameObjects를 사용하는 경우
// 예: using Photon.Pun;    // Photon PUN2를 사용하는 경우

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float speed = 5f; // 이동 속도 조절 변수

    // 스피드 업/다운 기능을 위한 변수
    public float maxSpeed = 15f; // 최대 이동 속도
    public float minSpeed = 1f;  // 최소 이동 속도
    public float speedChangeRate = 1f; // Q/E 키를 누를 때마다 속도가 변화하는 양

    // 플레이어가 움직일 수 있는 경계선 정의 (더 이상 하드코딩된 제한을 사용하지 않습니다)
    // 이전 X, Y축 제한 변수(minX, maxX, minY, maxY/fixedY)는 제거되었습니다.
    // 이제 플레이어는 화면 경계에 구애받지 않고 자유롭게 이동할 수 있습니다.

    // 현재 이 오브젝트가 로컬 플레이어(현재 클라이언트에서 직접 제어되는 플레이어)인지 여부를 나타냅니다.
    // 이 값은 사용하는 네트워킹 프레임워크에 의해 설정되어야 합니다.
    // 예: Unity Netcode의 NetworkBehaviour 클래스에는 IsLocalPlayer 속성이 있습니다.
    // 테스트 목적으로는 true로 설정할 수 있지만, 실제 게임에서는 네트워크 연결 시점에 결정됩니다.
    public bool isLocalPlayer = true; // 현재는 테스트를 위해 true로 설정되어 있습니다.

    private void Update()
    {
        // isLocalPlayer가 true일 때만 키보드 입력을 처리하고 움직임을 계산합니다.
        // 다른 클라이언트의 플레이어(원격 플레이어)는 네트워크를 통해 위치 정보를 수신하여 업데이트됩니다.
        if (isLocalPlayer)
        {
            // 스피드 업/다운 키 입력 감지
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // Q 키를 누르면 속도를 증가시키고 최대 속도를 넘지 않도록 제한
                speed = Mathf.Min(speed + speedChangeRate, maxSpeed);
                Debug.Log($"Speed increased to: {speed}");
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                // E 키를 누르면 속도를 감소시키고 최소 속도 아래로 내려가지 않도록 제한
                speed = Mathf.Max(speed - speedChangeRate, minSpeed);
                Debug.Log($"Speed decreased to: {speed}");
            }

            // 1️ 키보드 입력 감지
            float h = Input.GetAxis("Horizontal"); // ←, → 방향키
            float v = Input.GetAxis("Vertical");   // ↑, ↓ 방향키

            Debug.Log($"h: {h}, v: {v}");

            // 2️ 입력으로부터 방향 벡터 계산
            Vector2 direction = new Vector2(h, v);
            Debug.Log($"direction: {direction.x}, {direction.y}");

            // 3 현재 위치 가져오기
            Vector2 position = transform.position;

            // 4️ 새로운 위치 계산
            Vector2 newPosition = position + direction * speed * Time.deltaTime;

            // 5 새로운 위치를 적용 (더 이상 X, Y축 이동 제한을 적용하지 않습니다)
            // 플레이어는 이제 필드 내에서 입력에 따라 자유롭게 움직일 수 있습니다.
            // 단, 화면 경계를 벗어나도 제한되지 않으므로, 필요한 경우 카메라 경계 또는 화면 래핑 로직을 추가해야 합니다.
            transform.position = newPosition;

            // 7️ 이 새로운 위치를 네트워크를 통해 다른 모든 클라이언트에 동기화해야 합니다.
            // **이 부분은 사용하는 네트워킹 프레임워크의 API에 따라 코드를 작성해야 합니다.**
            //
            // [예시: Unity Netcode for GameObjects]
            // - NetworkBehaviour를 상속받고, [ServerRpc] 어트리뷰트를 가진 메서드를 호출하여
            //   서버에 플레이어의 새로운 위치(또는 입력 값)를 보냅니다.
            // - 서버는 이 정보를 다른 클라이언트들에게 [ClientRpc]를 통해 브로드캐스트하거나,
            //   NetworkVariable<Vector3>와 같은 동기화 변수를 사용하여 자동으로 처리할 수 있습니다.
            //
            // [예시: Photon PUN2]
            // - PhotonView 컴포넌트의 OnPhotonSerializeView 콜백에서 transform.position을 직렬화하여 동기화합니다.
            // - 또는 PhotonNetwork.RPC()를 사용하여 특정 함수를 원격으로 호출하여 위치를 업데이트할 수도 있습니다.
            //
            // SendPositionToNetwork(newPosition); // 여기에 네트워크 전송 로직이 들어갑니다.
        }
        else
        {
            // 이 오브젝트가 원격 플레이어(다른 클라이언트가 제어하는 플레이어)일 경우
            // 이 플레이어의 위치는 네트워크를 통해 수신된 데이터로 업데이트되어야 합니다.
            //
            // 일반적으로 네트워킹 프레임워크가 이를 자동으로 처리하거나,
            // 수신된 최신 위치를 이용하여 interpolation (보간)하여 더욱 부드러운 움직임을 만듭니다.
            // 수동으로 개발하는 경우, 이 오브젝트가 네트워크를 통해 새로운 위치 데이터를 받았을 때
            // transform.position = receivedNetworkPosition; 와 같이 업데이트하는 로직이 필요합니다.
        }

        // Time.deltaTime: 한 프레임당 흐른 시간 (속도 보정)
    }
}