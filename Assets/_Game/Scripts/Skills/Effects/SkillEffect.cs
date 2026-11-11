using System;
using Units;

namespace Skills
{
    [Serializable]
    public abstract class SkillEffect
    {
        public abstract void Apply(SkillContext context, UnitController target);
    }
}
