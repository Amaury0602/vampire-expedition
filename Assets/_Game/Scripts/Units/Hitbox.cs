using UnityEngine;

namespace Units
{
    public class Hitbox : MonoBehaviour
    {
        [field: SerializeField] public float Radius { get; private set; } = 0.5f;
        [field: SerializeField] public Vector2 Offset { get; private set; }

        [SerializeField] private Color _gizmoColor = new Color(1f, 0.3f, 0.3f, 1f);

        public Vector2 Center
        {
            get { return (Vector2)transform.position + Offset; }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(Center, Radius);
        }
    }
}
