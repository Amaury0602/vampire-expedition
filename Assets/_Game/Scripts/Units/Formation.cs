using UnityEngine;

namespace Units
{
    public static class Formation
    {
        private static readonly float GoldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f));

        public static Vector2 GetSlot(Vector2 center, int index, float spacing)
        {
            if (index == 0)
                return center;

            float radius = spacing * Mathf.Sqrt(index);
            float angle = index * GoldenAngle;
            return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
    }
}
