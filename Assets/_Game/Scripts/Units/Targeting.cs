using System.Collections.Generic;
using Battle;
using UnityEngine;

namespace Units
{
    public static class Targeting
    {
        private const float SwitchSqrRatio = 0.81f;

        public static bool IsValidTarget(UnitController target)
        {
            return target != null && target.isActiveAndEnabled && !target.Health.IsEmpty;
        }

        public static bool IsInRange(UnitController attacker, UnitController target, float range)
        {
            if (!IsValidTarget(target))
            {
                return false;
            }

            Vector2 offset = target.Hitbox.Center - attacker.Hitbox.Center;
            float reach = range + target.Hitbox.Radius;
            return offset.sqrMagnitude <= reach * reach;
        }

        public static UnitController FindTarget(UnitController unit, UnitController current)
        {
            Battlefield battlefield = unit.Combat.Battlefield;
            if (battlefield == null)
            {
                return null;
            }

            UnitController closest = FindClosestEnemy(unit, battlefield, out float closestSqrDistance);
            if (closest == null || closest == current || !IsValidTarget(current))
            {
                return closest;
            }

            float currentSqrDistance = (current.Rigidbody.position - unit.Rigidbody.position).sqrMagnitude;
            if (closestSqrDistance > currentSqrDistance * SwitchSqrRatio)
            {
                return current;
            }

            return closest;
        }

        private static UnitController FindClosestEnemy(UnitController unit, Battlefield battlefield, out float closestSqrDistance)
        {
            IReadOnlyList<UnitController> enemies = battlefield.GetEnemies(unit.Combat.Team);
            Vector2 position = unit.Rigidbody.position;

            UnitController closest = null;
            closestSqrDistance = float.MaxValue;

            for (int i = 0; i < enemies.Count; i++)
            {
                UnitController enemy = enemies[i];
                if (!IsValidTarget(enemy))
                {
                    continue;
                }

                float sqrDistance = (enemy.Rigidbody.position - position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closest = enemy;
                }
            }

            return closest;
        }
    }
}
