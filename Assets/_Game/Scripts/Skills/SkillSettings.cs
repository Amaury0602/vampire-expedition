using System.Collections.Generic;
using UnityEngine;

namespace Skills
{
    [CreateAssetMenu(menuName = "Skills/Skill", fileName = "Skill")]
    public class SkillSettings : ScriptableObject
    {
        public string Name;
        public Sprite Icon;
        public SkillTargeting Targeting = SkillTargeting.Instant;
        public SkillTargetFilter TargetFilter = SkillTargetFilter.Enemies;
        [Min(0f)] public float ManaCost = 1f;
        [Min(0f)] public float Radius = 1f;
        [SerializeReference] public List<SkillEffect> Effects = new List<SkillEffect>();
    }
}
