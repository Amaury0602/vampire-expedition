using UnityEngine;

namespace Projectiles
{
    [CreateAssetMenu(menuName = "Projectiles/Projectile Motion", fileName = "ProjectileMotion")]
    public class ProjectileMotion : ScriptableObject
    {
        [Min(0f)] public float Height = 1.5f;
        public AnimationCurve Curve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 4f), new Keyframe(0.5f, 1f, 0f, 0f), new Keyframe(1f, 0f, -4f, 0f));

        public float EvaluateHeight(float t)
        {
            return Height * Curve.Evaluate(Mathf.Clamp01(t));
        }
    }
}
