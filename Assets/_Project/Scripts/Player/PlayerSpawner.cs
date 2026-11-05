using Fusion;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkPrefabRef PlayerPrefab;

    private void Start()
    {
        var runner = PersistentBootstrap.ActiveRunner;
        if (runner == null || !runner.IsRunning) return;

        if (runner.IsSharedModeMasterClient)
        {
            foreach (var player in runner.ActivePlayers)
            {
                if (runner.GetPlayerObject(player) != null) continue;

                Debug.Log($"<color=cyan>[Spawner]</color> Spawning player {player}");
                runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity, player);
            }
        }
    }
}