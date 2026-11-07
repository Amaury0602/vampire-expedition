using Player;
using UnityEngine;

namespace Units
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class UnitController : MonoBehaviour
    {
        [field: Header("Movement")]
        [field: SerializeField] public MovementSettings MovementSettings { get; private set; } = new MovementSettings { MaxSpeed = 3f };
        [field: SerializeField] public NavigationSettings NavigationSettings { get; private set; } = new NavigationSettings();

        [field: Header("Debug")]
        [field: SerializeField] public StateManager StateManager { get; private set; } = new StateManager();
        [field: SerializeField] public MovementInfo MovementInfo { get; private set; } = new MovementInfo();

        public Rigidbody2D Rigidbody { get; private set; }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody2D>();
            Rigidbody.gravityScale = 0f;
            Rigidbody.freezeRotation = true;

            StateManager.AddState(new UnitIdleState(this));
            StateManager.AddState(new UnitMoveToTargetState(this));
            StateManager.Initialize<UnitIdleState>();
        }

        private void Update()
        {
            StateManager.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            StateManager.FixedUpdate(Time.fixedDeltaTime);
            UpdateMovementInfo();
        }

        private void OnDestroy()
        {
            StateManager.ClearEvents();
        }

        public void MoveToTarget(Vector2 position)
        {
            StateManager.GetState<UnitMoveToTargetState>().SetTargetPosition(position);
            StateManager.SetState<UnitMoveToTargetState>();
        }

        public void Steer(Vector2 desiredVelocity)
        {
            Vector2 steer = (desiredVelocity - Rigidbody.linearVelocity) * NavigationSettings.Acceleration;
            Rigidbody.AddForce(steer * Rigidbody.mass);
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
