using System;
using Battle;
using Damages;
using Items;
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
            if (Weapon == null || !IsValidTarget(target))
            {
                return false;
            }

            Vector2 offset = target.Hitbox.Center - _controller.Hitbox.Center;
            float reach = Weapon.Range + target.Hitbox.Radius;
            return offset.sqrMagnitude <= reach * reach;
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

            if (IsInRange(AttackTarget))
            {
                AttackTarget.Health.TakeDamage(CreateDamageInfo(AttackTarget));
            }

            AttackTarget = null;
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

        public static bool IsValidTarget(UnitController target)
        {
            return target != null && target.isActiveAndEnabled && !target.Health.IsEmpty;
        }
    }
}
