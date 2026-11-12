using System;
using Units;

namespace Skills
{
    [Serializable]
    public abstract class SkillEffect
    {
        public virtual void OnCast(SkillContext context)
        {
        }

        public virtual void Apply(SkillContext context, UnitController target)
        {
        }
    }
}
