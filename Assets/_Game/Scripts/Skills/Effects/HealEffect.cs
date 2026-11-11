using System;
using Units;
using UnityEngine;

namespace Skills
{
    [Serializable]
    public class HealEffect : SkillEffect
    {
        [Min(0f)] public float Amount = 1f;

        public override void Apply(SkillContext context, UnitController target)
        {
            target.Health.Gain(Amount);
        }
    }
}
