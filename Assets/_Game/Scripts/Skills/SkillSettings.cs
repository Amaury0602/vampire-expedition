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
        public SkillLevel[] Levels = { new SkillLevel() };

        public int MaxLevel
        {
            get { return Mathf.Max(1, Levels != null ? Levels.Length : 0); }
        }

        public int ClampLevel(int level)
        {
            return Mathf.Clamp(level, 1, MaxLevel);
        }

        public SkillLevel GetLevel(int level)
        {
            if (Levels == null || Levels.Length == 0)
            {
                return null;
            }

            return Levels[ClampLevel(level) - 1];
        }
    }
}
