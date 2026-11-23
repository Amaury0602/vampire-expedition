using System.Collections.Generic;
using Units;

namespace Worlds
{
    public class BattleRoom : Room
    {
        public IReadOnlyList<UnitController> Enemies { get; }

        public BattleRoom(int index, List<UnitController> enemies) : base(index)
        {
            Enemies = enemies;
        }
    }
}
