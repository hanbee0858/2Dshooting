using UnityEngine;

/// <summary>
/// 키 입력으로 플레이어 자동사격 토글(1=ON, 2=OFF).
/// </summary>
public class AutoBattleController : MonoBehaviour
{
    [SerializeField] private PlayerShooting _player;

    private void Reset()
    {
        if (_player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.GetComponent<PlayerShooting>();
        }
    }

    private void Update()
    {
        if (_player == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            _player.AutoBattleEnabled = true;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            _player.AutoBattleEnabled = false;
    }
}