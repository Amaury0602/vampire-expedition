using System;
using Damages;
using Units;
using UnityEngine;

namespace Skills
{
    [Serializable]
    public class DamageEffect : SkillEffect
    {
        [Min(0f)] public float Damage = 1f;
        public DamageType DamageTypes = DamageType.None;
        [Min(0f)] public float KnockbackPower;

        public override void Apply(SkillContext context, UnitController target)
        {
            Vector2 direction = target.Hitbox.Center - context.Origin;
            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();
            }
            else
            {
                direction = Vector2.zero;
            }

            GameObject source = context.Caster != null ? context.Caster.gameObject : null;
            target.Health.TakeDamage(new DamageInfo(Damage, direction, DamageTypes, KnockbackPower, source));
        }
    }
}
