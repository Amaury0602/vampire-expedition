using Damages;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Items/Weapon", fileName = "Weapon")]
    public class Weapon : Item
    {
        [Min(0f)] public float Damage = 1f;
        public DamageType DamageTypes = DamageType.Slash;
        [Min(0f)] public float KnockbackPower = 1f;
        [Min(0f)] public float Range = 0.5f;
        [Min(0.01f)] public float AttackSpeed = 1f;

        public float AttackCooldown
        {
            get { return 1f / AttackSpeed; }
        }
    }
}
