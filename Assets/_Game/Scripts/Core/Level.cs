using Battle;
using Menus;
using Skills;
using UnityEngine;

namespace Core
{
    public class Level : MonoBehaviour
    {
        [field: SerializeField] public Battlefield Battlefield { get; private set; }
        [field: SerializeField] public CombatClock Clock { get; private set; }
        [field: SerializeField] public ManaPool Mana { get; private set; }
        [field: SerializeField] public SkillCaster SkillCaster { get; private set; }

        public void Init()
        {
            Clock.Init();
            Mana.Init();
            SkillCaster.Init();
            Battlefield.Init();


            MenuManager.Show<CombatSkillsMenu>();
        }
    }
}
