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

        [field: Header("Debug")]
        [field: SerializeField] public StateManager StateManager { get; private set; } = new StateManager();
        [field: SerializeField] public MovementInfo MovementInfo { get; private set; } = new MovementInfo();

        public Rigidbody2D Rigidbody { get; private set; }
        public Health Health { get; private set; }
        public Hitbox Hitbox { get; private set; }
        public Equipment Equipment { get; private set; }
        public UnitSeparation Separation { get; private set; }
        public UnitSensor Sensor { get; private set; }

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

            StateManager.AddState(new UnitIdleState(this));
            StateManager.AddState(new UnitMoveToTargetState(this));
            StateManager.AddState(new UnitChaseState(this));
            StateManager.AddState(new UnitAttackState(this));
            StateManager.Initialize<UnitIdleState>();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            Combat.Update(dt);
            Sensor.Update(dt);
            UpdateCombatTransition();
            StateManager.Update(dt);
        }

        private void FixedUpdate()
        {
            StateManager.FixedUpdate(Time.fixedDeltaTime);
            UpdateMovementInfo();
        }

        private void OnDestroy()
        {
            Health.Emptied -= OnHealthEmptied;
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
            if (UnitCombat.IsValidTarget(Sensor.Target))
            {
                return;
            }

            StateManager.GetState<UnitMoveToTargetState>().SetTargetPosition(position);
            StateManager.SetState<UnitMoveToTargetState>();
        }

        public void Steer(Vector2 desiredVelocity)
        {
            Vector2 steer = (desiredVelocity - Rigidbody.linearVelocity) * NavigationSettings.Acceleration;
            Rigidbody.AddForce(steer * Rigidbody.mass);
        }

        public void OnGet()
        {
            Health.Fill();
            Combat.ResetCombat();
            Sensor.Clear();
            StateManager.SetState<UnitIdleState>();
        }

        public void OnRelease()
        {
            Combat.ResetCombat();
            Sensor.Clear();
            Rigidbody.linearVelocity = Vector2.zero;
        }

        private void UpdateCombatTransition()
        {
            if (!UnitCombat.IsValidTarget(Sensor.Target))
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
            Combat.Leave();
            Combat.ResetCombat();
            Sensor.Clear();

            if (TryGetComponent(out PooledObject _))
            {
                Pooler.Release(gameObject);
                return;
            }

            gameObject.SetActive(false);
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
