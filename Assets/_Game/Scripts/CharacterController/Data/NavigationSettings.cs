using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class NavigationSettings
    {
        public float Acceleration = 20f;
        public float StopDistance = 0.1f;
        public float RepathInterval = 0.3f;
        public float CornerReachDistance = 0.15f;

        [Header("Separation")]
        public LayerMask SeparationLayers = ~0;
        public float SeparationRadius = 0.6f;
        public float SeparationWeight = 1.5f;
    }
}
