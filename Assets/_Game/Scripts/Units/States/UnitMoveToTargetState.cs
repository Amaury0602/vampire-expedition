using UnityEngine;
using UnityEngine.AI;

namespace Units
{
    public class UnitMoveToTargetState : UnitBaseState
    {
        private const int MaxCorners = 32;
        private const float SampleRadius = 1f;

        private readonly NavMeshPath _path = new NavMeshPath();
        private readonly Vector3[] _corners = new Vector3[MaxCorners];
        private Vector2 _targetPosition;
        private int _cornerCount;
        private int _cornerIndex;
        private float _repathTimer;

        public UnitMoveToTargetState(UnitController controller) : base(controller) { }

        public void SetTargetPosition(Vector2 position)
        {
            _targetPosition = position;
            _cornerCount = 0;
            _repathTimer = 0f;
        }

        public override void UpdateState(float dt)
        {
            base.UpdateState(dt);

            float distance = Vector2.Distance(Controller.Rigidbody.position, _targetPosition);
            if (distance <= Controller.NavigationSettings.StopDistance)
            {
                StateManager.SetState<UnitIdleState>();
                return;
            }

            _repathTimer -= dt;
            if (_repathTimer <= 0f)
            {
                _repathTimer = Controller.NavigationSettings.RepathInterval;
                Repath();
            }
        }

        public override void FixedUpdateState(float dt)
        {
            base.FixedUpdateState(dt);

            Controller.Steer(ComputeDesiredVelocity());
        }

        private void Repath()
        {
            Vector2 from = Controller.Rigidbody.position;
            Vector2 to = _targetPosition;

            if (NavMesh.SamplePosition(from, out NavMeshHit fromHit, SampleRadius, NavMesh.AllAreas))
                from = fromHit.position;
            if (NavMesh.SamplePosition(to, out NavMeshHit toHit, SampleRadius, NavMesh.AllAreas))
                to = toHit.position;

            if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, _path) &&
                _path.status != NavMeshPathStatus.PathInvalid)
            {
                _cornerCount = _path.GetCornersNonAlloc(_corners);
                _cornerIndex = _cornerCount > 1 ? 1 : 0;
            }
            else
            {
                _cornerCount = 0;
            }
        }

        private Vector2 ComputeDesiredVelocity()
        {
            Vector2 position = Controller.Rigidbody.position;

            if (_cornerCount == 0)
                return (_targetPosition - position).normalized * Controller.MovementSettings.MaxSpeed;

            Vector2 toCorner = (Vector2)_corners[_cornerIndex] - position;
            while (toCorner.magnitude < Controller.NavigationSettings.CornerReachDistance && _cornerIndex < _cornerCount - 1)
            {
                _cornerIndex++;
                toCorner = (Vector2)_corners[_cornerIndex] - position;
            }

            return toCorner.normalized * Controller.MovementSettings.MaxSpeed;
        }
    }
}
