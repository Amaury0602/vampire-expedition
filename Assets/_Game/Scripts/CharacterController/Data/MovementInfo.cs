using UnityEngine;

namespace Player
{
    [System.Serializable]
    public class MovementInfo
    {
        public float NormalizedSpeed;
        public bool IsMoving;
        public Vector2 Direction;
    }
}