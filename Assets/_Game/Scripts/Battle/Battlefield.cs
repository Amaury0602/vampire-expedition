using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Battle
{
    public class Battlefield : MonoBehaviour
    {
        [SerializeField] private List<UnitController> _playerUnits = new List<UnitController>();
        [SerializeField] private List<UnitController> _otherUnits = new List<UnitController>();

        public IReadOnlyList<UnitController> PlayerUnits
        {
            get { return _playerUnits; }
        }

        public IReadOnlyList<UnitController> OtherUnits
        {
            get { return _otherUnits; }
        }

        private void Start()
        {
            JoinAll(_playerUnits, Team.Player);
            JoinAll(_otherUnits, Team.Other);
        }

        public void Add(UnitController unit, Team team)
        {
            List<UnitController> units = GetUnits(team);
            if (!units.Contains(unit))
            {
                units.Add(unit);
            }

            unit.Combat.Join(this, team);
        }

        public void Remove(UnitController unit)
        {
            _playerUnits.Remove(unit);
            _otherUnits.Remove(unit);
        }

        public IReadOnlyList<UnitController> GetEnemies(Team team)
        {
            if (team == Team.Player)
            {
                return _otherUnits;
            }

            return _playerUnits;
        }

        private List<UnitController> GetUnits(Team team)
        {
            if (team == Team.Player)
            {
                return _playerUnits;
            }

            return _otherUnits;
        }

        private void JoinAll(List<UnitController> units, Team team)
        {
            for (int i = units.Count - 1; i >= 0; i--)
            {
                if (units[i] == null)
                {
                    units.RemoveAt(i);
                    continue;
                }

                units[i].Combat.Join(this, team);
            }
        }
    }
}
