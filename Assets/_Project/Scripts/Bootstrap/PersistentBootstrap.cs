using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class PersistentBootstrap : MonoBehaviour
{
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private GameMode gameMode = GameMode.Shared;
    [SerializeField] private string sessionName = "PrototypeRoom";
    [SerializeField] private string mainSceneName = "MainScene";

    public static NetworkRunner ActiveRunner { get; private set; }

    private async void Awake()
    {
        if (ActiveRunner != null)
        {
            Destroy(gameObject);
            return;
        }

        var runner = Instantiate(runnerPrefab);
        DontDestroyOnLoad(runner.gameObject);
        ActiveRunner = runner;

        var sceneManager = runner.GetComponents(typeof(MonoBehaviour))
            .OfType<INetworkSceneManager>()
            .FirstOrDefault();
        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        var args = new StartGameArgs
        {
            GameMode = gameMode,
            SessionName = sessionName,
            SceneManager = sceneManager,
            Scene = SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(mainSceneName))
        };

        var result = await runner.StartGame(args);
        if (!result.Ok)
        {
            Debug.LogError($"[Bootstrap] StartGame failed: {result.ShutdownReason}");
            return;
        }
        Debug.Log("[Bootstrap] Runner started in MainScene.");
    }
}