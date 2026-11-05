using Fusion;
using UnityEngine;

public class PuzzleTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "BoatPuzzleScene";
    private PlayerInputHandler localPlayerInRange;

    private void OnTriggerEnter(Collider other)
    {
        var input = other.GetComponent<PlayerInputHandler>();
        if (input != null && input.HasInputAuthority)
        {
            localPlayerInRange = input;
            Debug.Log("[Prompt] Press E to enter the boat puzzle!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var input = other.GetComponent<PlayerInputHandler>();
        if (input != null && input.HasInputAuthority && input == localPlayerInRange)
        {
            localPlayerInRange = null;
            Debug.Log("[Prompt] Left interaction zone.");
        }
    }

    private void Update()
    {
        if (localPlayerInRange != null && localPlayerInRange.InteractPressed)
        {
            localPlayerInRange.ConsumeInteract();

            if (TransitionController.Instance != null)
            {
                TransitionController.Instance.EnterPuzzle(sceneToLoad);
            }
        }
    }

    private void OnGUI()
    {
        if (localPlayerInRange != null)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 24;
            style.normal.textColor = Color.yellow;

            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 + 50, 300, 50), "Press [E] to Interact", style);
        }
    }
}