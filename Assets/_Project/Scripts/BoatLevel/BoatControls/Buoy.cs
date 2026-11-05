using UnityEngine;

namespace Fusion
{
    public class Buoy : MonoBehaviour
    {
        public float Spring = 50;
        public float Damper = 0.05f;
        public bool Debug = false;

        private float _previousStrength;

        private Vector3 _lastForce;

        public bool ComputeAndApplyForce(Rigidbody body, WaveSample waves, int count, float time, out Vector3 force)
        {
            var position = transform.position;
            var localPosition = transform.localPosition;
            var desiredHeight = waves.GetHeight(position, time);
            var strength = Mathf.Clamp01(desiredHeight - position.y) * Spring;

            float damping = 0;
            if (_previousStrength != 0)
            {
                damping = strength - _previousStrength;
                damping = (damping * Damper) / Time.fixedDeltaTime; // NM
            }

            var forceMagnitude = Mathf.Clamp(strength + damping, 0, float.MaxValue);
            force = new Vector3(0, forceMagnitude, 0);

            _previousStrength = strength;
            _lastForce = force / count;

            if (strength > 0)
            {
                body.AddForceAtPosition(_lastForce, position, ForceMode.Force);
                return true;
            }
            return false;
        }
    }
}