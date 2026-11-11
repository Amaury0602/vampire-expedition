using System.Collections.Generic;
using Skills;
using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(menuName = "Characters/Character", fileName = "Character")]
    public class CharacterSettings : ScriptableObject
    {
        public string Name;
        public Sprite Portrait;
        public List<SkillSettings> Skills = new List<SkillSettings>();
    }
}
