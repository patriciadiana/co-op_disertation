using System.Collections;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionController : MonoBehaviour
{
    public static TransitionController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void EnterPuzzle(string sceneName)
    {
        var runner = PersistentBootstrap.ActiveRunner;
        if (runner == null || !runner.IsSceneAuthority) return;

        runner.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void ExitPuzzle(string sceneName = "BoatPuzzleScene")
    {
        var runner = PersistentBootstrap.ActiveRunner;
        if (runner == null || !runner.IsSceneAuthority) return;

        runner.LoadScene("MainScene", LoadSceneMode.Single);
    }
}