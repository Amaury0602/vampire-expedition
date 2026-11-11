using System;
using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Battle
{
    public class Battlefield : MonoBehaviour
    {
        [field: SerializeField] public CombatClock Clock { get; private set; }
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

        public event Action<UnitController> UnitJoined;
        public event Action<UnitController> UnitRemoved;

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
            UnitJoined?.Invoke(unit);
        }

        public void Remove(UnitController unit)
        {
            bool removed = _playerUnits.Remove(unit);
            removed |= _otherUnits.Remove(unit);

            if (removed)
            {
                UnitRemoved?.Invoke(unit);
            }
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
                UnitJoined?.Invoke(units[i]);
            }
        }
    }
}
