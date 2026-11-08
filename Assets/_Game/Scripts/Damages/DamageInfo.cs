using UnityEngine;

namespace Damages
{
    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly Vector2 Direction;
        public readonly DamageType Types;
        public readonly GameObject Source;

        public DamageInfo(float amount, Vector2 direction, DamageType types, GameObject source)
        {
            Amount = amount;
            Direction = direction;
            Types = types;
            Source = source;
        }

        public bool HasType(DamageType type)
        {
            return (Types & type) == type;
        }
    }
}
