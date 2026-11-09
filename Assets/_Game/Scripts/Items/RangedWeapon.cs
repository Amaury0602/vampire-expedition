using Projectiles;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Items/Ranged Weapon", fileName = "RangedWeapon")]
    public class RangedWeapon : Weapon
    {
        public Projectile Projectile;
        [Min(0.01f)] public float ProjectileSpeed = 10f;
    }
}
