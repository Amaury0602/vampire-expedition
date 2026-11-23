using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Worlds
{
    [CreateAssetMenu(menuName = "Worlds/Player Roster", fileName = "PlayerRoster")]
    public class PlayerRosterSettings : ScriptableObject
    {
        public List<UnitController> Units = new List<UnitController>();
    }
}
