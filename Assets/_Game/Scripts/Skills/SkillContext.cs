using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Skills
{
    public class SkillContext
    {
        public SkillSettings Settings;
        public UnitController Caster;
        public UnitController TargetUnit;
        public Vector2 Position;
        public readonly List<UnitController> Targets = new List<UnitController>();

        public Vector2 Origin
        {
            get
            {
                if (Settings != null && Settings.Targeting == SkillTargeting.Area)
                {
                    return Position;
                }

                return Caster != null ? Caster.Hitbox.Center : Position;
            }
        }

        public void Clear()
        {
            Settings = null;
            Caster = null;
            TargetUnit = null;
            Position = Vector2.zero;
            Targets.Clear();
        }
    }
}
