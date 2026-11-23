using System.Collections.Generic;
using Battle;
using Characters;
using Core;
using Items;
using Pooling;
using Skills;
using Units;
using UnityEngine;
using Worlds;

namespace Sequences
{
    public class InitPoolsHandler : Handler
    {
        private readonly int _defaultSize;

        public InitPoolsHandler(int defaultSize = 2)
        {
            _defaultSize = Mathf.Max(1, defaultSize);
        }

        protected override void OnEnter()
        {
            HashSet<GameObject> prefabs = new HashSet<GameObject>();

            Battlefield battlefield = Game.Level != null ? Game.Level.Battlefield : null;
            if (battlefield != null)
            {
                CollectUnits(battlefield.PlayerUnits, prefabs, false);
                CollectUnits(battlefield.OtherUnits, prefabs, false);
            }

            CollectWorld(Game.World, prefabs);

            foreach (GameObject prefab in prefabs)
            {
                Pooler.SetPool(prefab, GetPoolSize(prefab));
            }

            Continue();
        }

        private void CollectWorld(World world, HashSet<GameObject> prefabs)
        {
            if (world == null)
            {
                return;
            }

            if (world.Roster != null)
            {
                CollectUnits(world.Roster.Units, prefabs, true);
            }

            if (world.Settings != null)
            {
                CollectUnits(world.Settings.Enemies, prefabs, true);
            }
        }

        private void CollectUnits(IReadOnlyList<UnitController> units, HashSet<GameObject> prefabs, bool includeUnit)
        {
            for (int i = 0; i < units.Count; i++)
            {
                UnitController unit = units[i];
                if (unit == null)
                {
                    continue;
                }

                if (includeUnit)
                {
                    prefabs.Add(unit.gameObject);
                }

                CollectWeapon(unit.GetComponent<Equipment>(), prefabs);

                if (unit.TryGetComponent(out Hero hero))
                {
                    CollectCharacter(hero.Settings, prefabs);
                }
            }
        }

        private void CollectWeapon(Equipment equipment, HashSet<GameObject> prefabs)
        {
            if (equipment != null && equipment.Weapon is RangedWeapon ranged && ranged.Projectile != null)
            {
                prefabs.Add(ranged.Projectile.gameObject);
            }
        }

        private void CollectCharacter(CharacterSettings settings, HashSet<GameObject> prefabs)
        {
            if (settings == null || settings.Skills == null)
            {
                return;
            }

            for (int i = 0; i < settings.Skills.Count; i++)
            {
                CollectSkill(settings.Skills[i], prefabs);
            }
        }

        private void CollectSkill(SkillSettings skill, HashSet<GameObject> prefabs)
        {
            if (skill == null || skill.Levels == null)
            {
                return;
            }

            for (int i = 0; i < skill.Levels.Length; i++)
            {
                SkillLevel level = skill.Levels[i];
                if (level == null || level.Effects == null)
                {
                    continue;
                }

                for (int j = 0; j < level.Effects.Count; j++)
                {
                    if (level.Effects[j] is SpawnFxEffect spawnFx && spawnFx.Prefab != null)
                    {
                        prefabs.Add(spawnFx.Prefab.gameObject);
                    }
                }
            }
        }

        private int GetPoolSize(GameObject prefab)
        {
            if (prefab.TryGetComponent(out PooledObject pooled))
            {
                return pooled.PoolSize;
            }

            return _defaultSize;
        }
    }
}
