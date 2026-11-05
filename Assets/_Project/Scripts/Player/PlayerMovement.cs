using Fusion;
using Unity.Burst.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;

    private NetworkCharacterController networkController;
    private PlayerInputHandler input;
    private PlayerLook look;

    private void Awake()
    {
        networkController = GetComponent<NetworkCharacterController>();
        input = GetComponent<PlayerInputHandler>();
        look = GetComponent<PlayerLook>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (networkController.Grounded && input.JumpPressed)
        {
            networkController.Jump(true);
        }
        input.ConsumeJump();

        Vector3 moveDirection = (transform.forward * input.Move.y + transform.right * input.Move.x).normalized;
        Vector3 velocity = moveDirection * moveSpeed;

        networkController.Move(velocity * Runner.DeltaTime);

        if (look != null)
        {
            transform.rotation = Quaternion.Euler(0f, look.Yaw, 0f);
        }
    }
}