using UnityEngine;

namespace Units
{
    public class UnitIdleState : UnitBaseState
    {
        public UnitIdleState(UnitController controller) : base(controller) { }

        public override void FixedUpdateState(float dt)
        {
            base.FixedUpdateState(dt);

            Controller.Steer(Vector2.zero);
        }
    }
}
