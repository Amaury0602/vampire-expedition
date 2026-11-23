using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Worlds
{
    [CreateAssetMenu(menuName = "Worlds/World", fileName = "World")]
    public class WorldSettings : ScriptableObject
    {
        public string Name;
        public int RoomCount = 5;
        public List<UnitController> Enemies = new List<UnitController>();
        public int MinEnemiesPerRoom = 3;
        public int MaxEnemiesPerRoom = 5;

        public List<UnitController> RollEnemies()
        {
            List<UnitController> result = new List<UnitController>();
            List<UnitController> candidates = new List<UnitController>();
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i] != null)
                {
                    candidates.Add(Enemies[i]);
                }
            }

            if (candidates.Count == 0)
            {
                return result;
            }

            int min = Mathf.Max(0, MinEnemiesPerRoom);
            int max = Mathf.Max(min, MaxEnemiesPerRoom);
            int count = Random.Range(min, max + 1);
            for (int i = 0; i < count; i++)
            {
                result.Add(candidates[Random.Range(0, candidates.Count)]);
            }

            return result;
        }
    }
}
