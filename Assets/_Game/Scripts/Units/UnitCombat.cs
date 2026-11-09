using System;
using Battle;
using Damages;
using Items;
using Pooling;
using Projectiles;
using UnityEngine;

namespace Units
{
    [Serializable]
    public class UnitCombat
    {
        [field: SerializeField] public Team Team { get; private set; }

        public Battlefield Battlefield { get; private set; }
        public UnitController AttackTarget { get; private set; }
        public bool IsAttacking { get; private set; }

        public Weapon Weapon
        {
            get { return _controller.Equipment != null ? _controller.Equipment.Weapon : null; }
        }

        public bool CanStartAttack
        {
            get { return Weapon != null && !IsAttacking && _cooldown <= 0f; }
        }

        public event Action AttackStarted;

        private UnitController _controller;
        private float _cooldown;

        public void Initialize(UnitController controller)
        {
            _controller = controller;
        }

        public void Join(Battlefield battlefield, Team team)
        {
            Battlefield = battlefield;
            Team = team;
        }

        public void Leave()
        {
            if (Battlefield != null)
            {
                Battlefield.Remove(_controller);
            }

            Battlefield = null;
        }

        public void Update(float dt)
        {
            if (_cooldown <= 0f)
            {
                return;
            }

            _cooldown -= dt;
            if (_cooldown <= 0f)
            {
                IsAttacking = false;
            }
        }

        public bool IsInRange(UnitController target)
        {
            if (Weapon == null)
            {
                return false;
            }

            return Targeting.IsInRange(_controller, target, Weapon.Range);
        }

        public void StartAttack(UnitController target)
        {
            if (!CanStartAttack)
            {
                return;
            }

            AttackTarget = target;
            IsAttacking = true;
            _cooldown = Weapon.AttackCooldown;
            AttackStarted?.Invoke();
        }

        public void Hit()
        {
            if (!IsAttacking)
            {
                return;
            }

            IsAttacking = false;

            if (Weapon is RangedWeapon rangedWeapon)
            {
                Shoot(rangedWeapon, AttackTarget);
            }
            else if (IsInRange(AttackTarget))
            {
                AttackTarget.Health.TakeDamage(CreateDamageInfo(AttackTarget));
            }

            AttackTarget = null;
        }

        private void Shoot(RangedWeapon weapon, UnitController target)
        {
            if (weapon.Projectile == null || !Targeting.IsValidTarget(target))
            {
                return;
            }

            Projectile projectile = Pooler.Get(weapon.Projectile, _controller.Hitbox.Center, Quaternion.identity);
            projectile.Launch(target, weapon.ProjectileSpeed, weapon.Damage, weapon.DamageTypes, weapon.KnockbackPower, _controller.gameObject);
        }

        private DamageInfo CreateDamageInfo(UnitController target)
        {
            Vector2 direction = (target.Hitbox.Center - _controller.Hitbox.Center).normalized;
            return new DamageInfo(Weapon.Damage, direction, Weapon.DamageTypes, Weapon.KnockbackPower, _controller.gameObject);
        }

        public void CancelAttack()
        {
            IsAttacking = false;
            AttackTarget = null;
        }

        public void ResetCombat()
        {
            CancelAttack();
            _cooldown = 0f;
        }
    }
}
