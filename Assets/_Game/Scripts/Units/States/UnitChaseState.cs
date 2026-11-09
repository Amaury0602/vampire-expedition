using Player;

namespace Units
{
    public class UnitChaseState : UnitMoveToTargetState
    {
        public UnitChaseState(UnitController controller) : base(controller) { }

        public override void EnterState(StateParameters parameters = null)
        {
            if (Targeting.IsValidTarget(Controller.Sensor.Target))
            {
                SetTargetPosition(Controller.Sensor.Target.Rigidbody.position);
            }

            base.EnterState(parameters);
        }

        public override void UpdateState(float dt)
        {
            UnitController target = Controller.Sensor.Target;
            if (!Targeting.IsValidTarget(target))
            {
                StateManager.SetState<UnitIdleState>();
                return;
            }

            if (Controller.Combat.IsInRange(target))
            {
                StateManager.SetState<UnitAttackState>();
                return;
            }

            UpdateTargetPosition(target.Rigidbody.position);
            base.UpdateState(dt);
        }

        protected override void OnTargetReached()
        {
        }
    }
}
