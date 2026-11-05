using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    public bool GasOn => NetworkedThrottle != 0;
    private bool _exiting;

    [Networked] private float NetworkedSteering { get; set; }
    [Networked] private float NetworkedThrottle { get; set; }

    private enum ControlRole { Steering, Throttle, Both }

    private float _lastSentSteer;
    private float _lastSentThrottle;
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
            UpdateControls(Runner.DeltaTime);
        }
    }

    private ControlRole GetLocalRole()
    {
        int count = 0;
        int lowestId = int.MaxValue;
        foreach (var player in Runner.ActivePlayers)
        {
            count++;
            lowestId = Mathf.Min(lowestId, player.PlayerId);
        }

        if (count <= 1) return ControlRole.Both;
        return Runner.LocalPlayer.PlayerId == lowestId ? ControlRole.Steering : ControlRole.Throttle;
    }

    private void Update()
    {
        if (Object == null || !Object.IsValid) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        var role = GetLocalRole();

        if (role != ControlRole.Throttle)
        {
            float steer = 0;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) steer = 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) steer = -1f;

            if (steer != _lastSentSteer)
            {
                _lastSentSteer = steer;
                Rpc_SetSteering(steer);
            }
        }

        if (role != ControlRole.Steering)
        {
            float throttle = 0;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) throttle = 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) throttle = -1f;

            if (throttle != _lastSentThrottle)
            {
                _lastSentThrottle = throttle;
                Rpc_SetThrottle(throttle);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_SetSteering(float value)
    {
        NetworkedSteering = value;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_SetThrottle(float value)
    {
        NetworkedThrottle = value;
    }

    private void OnGUI()
    {
        if (Object == null || !Object.IsValid) return;

        string text = GetLocalRole() switch
        {
            ControlRole.Steering => "You control: STEERING  [A] / [D]",
            ControlRole.Throttle => "You control: ENGINE  [W] / [S]",
            _ => "You control: STEERING [A]/[D] and ENGINE [W]/[S]",
        };

        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
        };
        style.normal.textColor = Color.yellow;

        GUI.Label(new Rect(Screen.width / 2 - 250, 20, 500, 40), text, style);
    }

    private void UpdateControls(float deltaTime)
    {
        if (_body == null) return;

        float rudderInput = NetworkedSteering;
        float engineInput = NetworkedThrottle;

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
            NetworkedThrottle = 0;
            RudderRotation = 0;

            Debug.Log("Boat Hit Obstacle! Resetting...");
        }
    }

    public void CompletePuzzle()
    {
        Rpc_RequestExitPuzzle();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void Rpc_RequestExitPuzzle()
    {
        if (_exiting || !Runner.IsSceneAuthority) return;

        _exiting = true;
        Runner.LoadScene("MainScene", LoadSceneMode.Single);
    }
}