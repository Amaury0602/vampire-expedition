using System;
using UnityEngine;

namespace Skills
{
    [Serializable]
    public class SkillFxLevelVariant
    {
        public GameObject Target;
        [Min(1)] public int MinLevel = 1;
        [Min(0)] public int MaxLevel;

        public bool Matches(int level)
        {
            return level >= MinLevel && (MaxLevel <= 0 || level <= MaxLevel);
        }
    }
}
