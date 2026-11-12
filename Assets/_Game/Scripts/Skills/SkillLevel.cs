using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skills
{
    [Serializable]
    public class SkillLevel
    {
        [Min(0f)] public float ManaCost = 1f;
        [Min(0f)] public float Radius = 1f;
        [SerializeReference] public List<SkillEffect> Effects = new List<SkillEffect>();
    }
}
