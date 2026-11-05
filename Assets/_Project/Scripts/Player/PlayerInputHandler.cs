using Fusion;
using UnityEngine;

public class PlayerInputHandler : NetworkBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool InteractPressed { get; private set; }

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        // Movement
        controls.Player.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => Move = Vector2.zero;

        // Look
        controls.Player.Look.performed += ctx => Look = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => Look = Vector2.zero;

        // Actions
        controls.Player.Jump.performed += ctx => JumpPressed = true;
        controls.Player.Interact.performed += ctx => InteractPressed = true;
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
            controls.Player.Enable();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        controls.Player.Disable();
    }

    //private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    public void ConsumeJump() => JumpPressed = false;
    public void ConsumeInteract() => InteractPressed = false;
}