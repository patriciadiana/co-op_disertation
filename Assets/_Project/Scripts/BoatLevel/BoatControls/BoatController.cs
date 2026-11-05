using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkTransform))]
public class BoatController : NetworkBehaviour
{
    [Header("Physics Settings")]
    public float EnginePower = 10;
    public float RudderSpeed = 90;
    public float RudderMaxAngle = 30;
    public float RudderDrag = 10;
    public float RudderArea = 2;

    [Header("References")]
    public Transform Rudder;
    public WaveSample Waves;

    public float RudderRotation;
    public bool GasOn => localThrottle != 0;

    [Networked] private float NetworkedSteering { get; set; }

    private float localThrottle;
    private float _time;

    private Rigidbody _body;
    private Hull _hull;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Start()
    {
        _body = GetComponent<Rigidbody>();
        _hull = GetComponent<Hull>();

        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            var kb = Keyboard.current;
            localThrottle = 0;
            float steer = 0;

            if (kb != null)
            {
                // Throttle Controls (W/S)
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) localThrottle = 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) localThrottle = -1f;

                // Steering Controls (A/D)
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) steer = 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) steer = -1f;
            }

            NetworkedSteering = steer;

            UpdateControls(Runner.DeltaTime);
        }

        /* 
        else
        {
            var kb = Keyboard.current;
            float steer = 0;
            if (kb != null)
            {
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) steer = 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) steer = -1f;
            }

            Rpc_SendSteering(steer);
        }
        */
    }

    /* 
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_SendSteering(float steer)
    {
        NetworkedSteering = steer;
    }
    */

    private void UpdateControls(float deltaTime)
    {
        if (_body == null) return;

        float rudderInput = NetworkedSteering;
        float engineInput = localThrottle;

        if (Mathf.Abs(rudderInput) > 0.1f)
        {
            RudderRotation -= rudderInput * RudderSpeed * deltaTime;
            RudderRotation = Mathf.Clamp(RudderRotation, -RudderMaxAngle, RudderMaxAngle);
        }
        else
        {
            RudderRotation *= (1 - deltaTime * RudderDrag);
        }

        var rudderEuler = new Vector3(0, RudderRotation, 0);
        var rudderPosition = Rudder.position;

        _time = Runner.SimulationTime;
        float waveHeight = Waves.GetHeight(rudderPosition, _time);
        bool rudderInWater = waveHeight >= rudderPosition.y;

        if (rudderInWater)
        {
            Rudder.localRotation = Quaternion.Euler(rudderEuler);
            var rudderRight = Rudder.right;
            Hull.AddDragForce(transform, _body, Rudder.localPosition, rudderRight, RudderArea);

            Vector3 engineForce = transform.forward * EnginePower * engineInput;
            _body.AddForce(engineForce);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (HasStateAuthority && collision.gameObject.CompareTag("Obstacle"))
        {
            Rpc_ResetPuzzle();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_ResetPuzzle()
    {
        if (HasStateAuthority && _body != null)
        {
            _body.position = _startPosition;
            _body.rotation = _startRotation;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;

            NetworkedSteering = 0;
            localThrottle = 0;
            RudderRotation = 0;

            Debug.Log("Boat Hit Obstacle! Resetting...");
        }
    }

    public void CompletePuzzle()
    {
        Rpc_RequestExitPuzzle();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_RequestExitPuzzle()
    {
        if (Runner.IsSharedModeMasterClient && TransitionController.Instance != null)
        {
            TransitionController.Instance.ExitPuzzle();
        }
    }
}