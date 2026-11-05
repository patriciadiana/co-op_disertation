using Fusion;
using Photon.Realtime;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkPrefabRef PlayerPrefab;
    [SerializeField] private Transform spawnPosition;

    private void Update()
    {
        var runner = PersistentBootstrap.ActiveRunner;

        if (runner == null || !runner.IsRunning || runner.LocalPlayer == PlayerRef.None) return;

        enabled = false;

        var local = runner.LocalPlayer;
        if (runner.TryGetPlayerObject(local, out var existing) && existing != null) return;

        Debug.Log($"<color=cyan>[Spawner]</color> Spawning player {local}");

        var obj = runner.Spawn(PlayerPrefab, spawnPosition.position, Quaternion.identity, local);
        runner.SetPlayerObject(local, obj);
    }
}