using System;
using Pooling;
using Units;
using UnityEngine;

namespace Skills
{
    [Serializable]
    public class SpawnFxEffect : SkillEffect
    {
        public SkillFx Prefab;
        public SkillFxAnchor Anchor = SkillFxAnchor.Origin;
        public SkillFxScaleMode ScaleMode = SkillFxScaleMode.Radius;
        [Min(0f)] public float ScaleMultiplier = 1f;

        public override void OnCast(SkillContext context)
        {
            if (Anchor == SkillFxAnchor.EachTarget)
            {
                return;
            }

            if (Anchor == SkillFxAnchor.Caster && context.Caster != null)
            {
                Spawn(context, context.Caster.Hitbox.Center);
                return;
            }

            Spawn(context, context.Origin);
        }

        public override void Apply(SkillContext context, UnitController target)
        {
            if (Anchor != SkillFxAnchor.EachTarget)
            {
                return;
            }

            Spawn(context, target.Hitbox.Center);
        }

        private void Spawn(SkillContext context, Vector2 position)
        {
            if (Prefab == null)
            {
                return;
            }

            SkillFx fx = Pooler.Get(Prefab, position, Quaternion.identity);
            fx.Play(position, GetScale(context), context.Level);
        }

        private float GetScale(SkillContext context)
        {
            if (ScaleMode == SkillFxScaleMode.Radius && context.Radius > 0f)
            {
                return context.Radius * ScaleMultiplier;
            }

            return ScaleMultiplier;
        }
    }
}
