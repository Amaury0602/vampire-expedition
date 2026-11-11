using System;
using System.Threading;
using Battle;
using Cysharp.Threading.Tasks;
using Damages;
using Items;
using Player;
using Pooling;
using UnityEngine;

namespace Units
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Hitbox))]
    [RequireComponent(typeof(Equipment))]
    public class UnitController : MonoBehaviour, IPoolable
    {
        [field: Header("Movement")]
        [field: SerializeField] public MovementSettings MovementSettings { get; private set; } = new MovementSettings { MaxSpeed = 3f };
        [field: SerializeField] public NavigationSettings NavigationSettings { get; private set; } = new NavigationSettings();

        [field: Header("Combat")]
        [field: SerializeField] public UnitCombat Combat { get; private set; } = new UnitCombat();
        [SerializeField] private float _knockbackDuration = 0.2f;
        [SerializeField] private float _deathDuration = 2f;

        [field: Header("Debug")]
        [field: SerializeField] public StateManager StateManager { get; private set; } = new StateManager();
        [field: SerializeField] public MovementInfo MovementInfo { get; private set; } = new MovementInfo();

        public Rigidbody2D Rigidbody { get; private set; }
        public Health Health { get; private set; }
        public Hitbox Hitbox { get; private set; }
        public Equipment Equipment { get; private set; }
        public UnitSeparation Separation { get; private set; }
        public UnitSensor Sensor { get; private set; }
        public bool IsKnockedBack
        {
            get { return _knockbackTimer > 0f; }
        }
        public bool IsDead { get; private set; }
        public CombatClock Clock { get; private set; }

        public event Action Died;
        public event Action<CombatClock> ClockChanged;

        private bool _isTicking;

        private float _knockbackTimer;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Rigidbody.gravityScale = 0f;
            Rigidbody.freezeRotation = true;

            Health = GetComponent<Health>();
            Hitbox = GetComponent<Hitbox>();
            Equipment = GetComponent<Equipment>();

            Separation = new UnitSeparation(Rigidbody, NavigationSettings, MovementSettings);
            Combat.Initialize(this);
            Sensor = new UnitSensor(this);

            Health.Emptied += OnHealthEmptied;
            Health.Damaged += OnDamaged;

            StateManager.AddState(new UnitIdleState(this));
            StateManager.AddState(new UnitMoveToTargetState(this));
            StateManager.AddState(new UnitChaseState(this));
            StateManager.AddState(new UnitAttackState(this));
            StateManager.AddState(new UnitDeadState(this));
            StateManager.Initialize<UnitIdleState>();
        }

        public void AttachClock(CombatClock clock)
        {
            if (clock == null)
            {
                Debug.LogWarning($"[UnitController] '{name}' joined a battlefield without CombatClock.", this);
            }

            if (Clock != clock)
            {
                DetachClock();
                Clock = clock;
                ClockChanged?.Invoke(Clock);
            }

            StartTicking();
        }

        public void DetachClock()
        {
            StopTicking();

            if (Clock == null)
            {
                return;
            }

            Clock = null;
            ClockChanged?.Invoke(null);
        }

        public void StopTicking()
        {
            if (!_isTicking || Clock == null)
            {
                _isTicking = false;
                return;
            }

            Clock.Ticked -= OnTicked;
            Clock.FixedTicked -= OnFixedTicked;
            _isTicking = false;
        }

        private void StartTicking()
        {
            if (_isTicking || Clock == null)
            {
                return;
            }

            Clock.Ticked += OnTicked;
            Clock.FixedTicked += OnFixedTicked;
            _isTicking = true;
        }

        private void OnTicked(float dt)
        {
            if (IsDead)
            {
                return;
            }

            Combat.Update(dt);
            Sensor.Update(dt);
            UpdateCombatTransition();
            StateManager.Update(dt);
        }

        private void OnFixedTicked(float dt)
        {
            if (IsDead)
            {
                return;
            }

            if (_knockbackTimer > 0f)
            {
                _knockbackTimer -= dt;
            }

            StateManager.FixedUpdate(dt);
            UpdateMovementInfo();
        }

        private void OnDestroy()
        {
            DetachClock();
            Health.Emptied -= OnHealthEmptied;
            Health.Damaged -= OnDamaged;
            StateManager.ClearEvents();
        }

        private void OnDrawGizmosSelected()
        {
            Equipment equipment = GetComponent<Equipment>();
            Hitbox hitbox = GetComponent<Hitbox>();
            if (equipment == null || hitbox == null || equipment.Weapon == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hitbox.Center, equipment.Weapon.Range);
        }

        public void MoveToTarget(Vector2 position)
        {
            if (IsDead || Targeting.IsValidTarget(Sensor.Target))
            {
                return;
            }

            StateManager.GetState<UnitMoveToTargetState>().SetTargetPosition(position);
            StateManager.SetState<UnitMoveToTargetState>();
        }

        public void Steer(Vector2 desiredVelocity)
        {
            if (IsKnockedBack)
            {
                return;
            }

            Vector2 steer = (desiredVelocity - Rigidbody.linearVelocity) * NavigationSettings.Acceleration;
            Rigidbody.AddForce(steer * Rigidbody.mass);
        }

        public void OnGet()
        {
            IsDead = false;
            Rigidbody.simulated = true;
            Health.Fill();
            Combat.ResetCombat();
            Sensor.Clear();
            StateManager.SetState<UnitIdleState>();
        }

        public void OnRelease()
        {
            DetachClock();
            Combat.ResetCombat();
            Sensor.Clear();
            Rigidbody.linearVelocity = Vector2.zero;
            _knockbackTimer = 0f;
        }

        public void ApplyKnockback(Vector2 direction, float power)
        {
            if (power <= 0f || direction == Vector2.zero)
            {
                return;
            }

            Rigidbody.AddForce(direction.normalized * power, ForceMode2D.Impulse);
            _knockbackTimer = _knockbackDuration;
        }

        private void OnDamaged(DamageInfo damage)
        {
            ApplyKnockback(damage.Direction, damage.KnockbackPower);
        }

        private void UpdateCombatTransition()
        {
            if (!Targeting.IsValidTarget(Sensor.Target))
            {
                return;
            }

            BaseState state = StateManager.CurrentState;
            if (state is UnitIdleState || (state is UnitMoveToTargetState && state is not UnitChaseState))
            {
                StateManager.SetState<UnitChaseState>();
            }
        }

        private void OnHealthEmptied()
        {
            Die();
        }

        private void Die()
        {
            CombatClock clock = Clock;
            IsDead = true;
            Combat.Leave();
            Combat.ResetCombat();
            Sensor.Clear();
            StateManager.SetState<UnitDeadState>();

            Rigidbody.linearVelocity = Vector2.zero;
            Rigidbody.simulated = false;
            _knockbackTimer = 0f;

            Died?.Invoke();

            if (clock != null)
            {
                ReleaseAfterDeathAsync(clock, this.GetCancellationTokenOnDestroy()).Forget();
            }
            else
            {
                Pooler.Release(gameObject, _deathDuration);
            }
        }

        private async UniTaskVoid ReleaseAfterDeathAsync(CombatClock clock, CancellationToken token)
        {
            try
            {
                await clock.Delay(_deathDuration, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (IsDead && isActiveAndEnabled)
            {
                Pooler.Release(gameObject);
            }
        }

        private void UpdateMovementInfo()
        {
            Vector2 velocity = Rigidbody.linearVelocity;
            float speed = velocity.magnitude;

            MovementInfo.IsMoving = speed > 0.05f;
            MovementInfo.NormalizedSpeed = MovementSettings.MaxSpeed > 0f
                ? Mathf.Clamp01(speed / MovementSettings.MaxSpeed)
                : 0f;

            if (MovementInfo.IsMoving)
                MovementInfo.Direction = velocity / speed;
        }
    }
}
