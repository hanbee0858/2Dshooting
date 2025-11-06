// AutoBattleController.cs
using UnityEngine;

public class AutoBattleController : MonoBehaviour
{
    public PlayerShooting playerShooting;
    public KeyCode toggleKey = KeyCode.T;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (playerShooting != null)
            {
                playerShooting.autoBattleEnabled = !playerShooting.autoBattleEnabled;
                Debug.Log($"[AutoBattle] {(playerShooting.autoBattleEnabled ? "ON" : "OFF")}");
            }
        }
    }
}