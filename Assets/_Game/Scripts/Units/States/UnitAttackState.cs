using UnityEngine;

namespace Units
{
    public class UnitAttackState : UnitBaseState
    {
        public UnitAttackState(UnitController controller) : base(controller) { }

        public override void UpdateState(float dt)
        {
            base.UpdateState(dt);

            UnitController target = Controller.Sensor.Target;
            if (!UnitCombat.IsValidTarget(target))
            {
                StateManager.SetState<UnitIdleState>();
                return;
            }

            bool isInRange = Controller.Combat.IsInRange(target);
            if (!isInRange)
            {
                if (!Controller.Combat.IsAttacking)
                {
                    StateManager.SetState<UnitChaseState>();
                }

                return;
            }

            if (Controller.Combat.CanStartAttack)
            {
                Controller.Combat.StartAttack(target);
            }
        }

        public override void FixedUpdateState(float dt)
        {
            base.FixedUpdateState(dt);

            Controller.Steer(Vector2.zero);
        }

        public override void ExitState()
        {
            Controller.Combat.CancelAttack();

            base.ExitState();
        }
    }
}
