using System.Collections.Generic;
using Core;
using Pooling;
using Units;
using UnityEngine;

namespace Battle
{
    public class Deployment : MonoBehaviour
    {
        [field: SerializeField] public Rect PlayerZone { get; private set; } = new Rect(-8f, -3f, 5f, 6f);
        [field: SerializeField] public Rect EnemyZone { get; private set; } = new Rect(3f, -3f, 5f, 6f);
        [field: SerializeField] public float Spacing { get; private set; } = 1f;

        private readonly List<UnitController> _players = new List<UnitController>();
        private readonly List<UnitController> _enemies = new List<UnitController>();

        public void SpawnPlayers(IReadOnlyList<UnitController> prefabs)
        {
            Spawn(prefabs, PlayerZone, Team.Player, _players);
        }

        public void PlacePlayers()
        {
            IReadOnlyList<UnitController> units = Game.Level.Battlefield.PlayerUnits;
            for (int i = 0; i < units.Count; i++)
            {
                units[i].ResetAt(GetSlot(PlayerZone, i));
            }
        }

        public void SpawnEnemies(IReadOnlyList<UnitController> prefabs)
        {
            Spawn(prefabs, EnemyZone, Team.Other, _enemies);
        }

        public void ClearEnemies()
        {
            ReleaseAll(_enemies, true);
        }

        public void ClearDeadPlayers()
        {
            ReleaseAll(_players, false);
        }

        private void Spawn(IReadOnlyList<UnitController> prefabs, Rect zone, Team team, List<UnitController> spawned)
        {
            Battlefield battlefield = Game.Level.Battlefield;
            for (int i = 0; i < prefabs.Count; i++)
            {
                if (prefabs[i] == null)
                {
                    continue;
                }

                Vector2 position = GetSlot(zone, i);
                UnitController unit = Pooler.Get(prefabs[i], position, Quaternion.identity);
                unit.ResetAt(position);
                battlefield.Add(unit, team);
                spawned.Add(unit);
            }
        }

        private void ReleaseAll(List<UnitController> units, bool includeAlive)
        {
            Battlefield battlefield = Game.Level.Battlefield;
            for (int i = units.Count - 1; i >= 0; i--)
            {
                UnitController unit = units[i];
                if (unit == null)
                {
                    units.RemoveAt(i);
                    continue;
                }

                if (!unit.IsDead && !includeAlive)
                {
                    continue;
                }

                units.RemoveAt(i);
                battlefield.Remove(unit);

                if (unit.gameObject.activeSelf)
                {
                    Pooler.Release(unit.gameObject);
                }
            }
        }

        private Vector2 GetSlot(Rect zone, int index)
        {
            Vector2 slot = Formation.GetSlot(zone.center, index, Spacing);
            slot.x = Mathf.Clamp(slot.x, zone.xMin, zone.xMax);
            slot.y = Mathf.Clamp(slot.y, zone.yMin, zone.yMax);
            return slot;
        }

        private void OnDrawGizmos()
        {
            DrawZone(PlayerZone, new Color(0.3f, 0.6f, 1f, 1f));
            DrawZone(EnemyZone, new Color(1f, 0.3f, 0.3f, 1f));
        }

        private void DrawZone(Rect zone, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(zone.center, zone.size);
        }
    }
}
