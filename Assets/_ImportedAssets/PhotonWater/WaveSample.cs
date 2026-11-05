using System;
using UnityEngine;

namespace Fusion
{

    [CreateAssetMenu(menuName = "Fusion/Forecast/WaveSample")]
    public class WaveSample : ScriptableObject
    {
        public Senoid[] WaveSources;

        public float GetHeight(Vector3 position, float time)
        {
            // reset on first sample
            float h = 0;
            for (int s = 0; s < WaveSources.Length; s++)
            {
                var source = WaveSources[s];
                float axis = default;
                switch (source.Axis)
                {
                    case Axis.X:
                        axis = position.x;
                        break;
                    case Axis.Z:
                        axis = position.z;
                        break;
                    case Axis.XZ:
                        axis = position.x + position.z;
                        break;
                }
                float k = (2 * Mathf.PI) / source.Wavelength;
                // accumulate all sources
                h += source.Amplitude * Mathf.Sin((axis - time * source.Speed) * k);
            }
            return h;
        }
        public bool CheckUnderwater(Vector3 position, float time, out float height)
        {
            height = GetHeight(position, time);
            return position.y <= height;
        }
    }

    public enum Axis
    {
        X, XZ, Z
    }

    [Serializable]
    public class Senoid
    {
        public float Speed = 1;
        public float Wavelength = 10;
        public float Amplitude = 0.25f;
        public Axis Axis = Axis.X;
    }
}
