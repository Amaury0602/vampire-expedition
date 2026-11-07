using Player;

namespace Units
{
    public abstract class UnitBaseState : BaseState
    {
        protected readonly UnitController Controller;
        protected StateManager StateManager
        {
            get { return Controller.StateManager; }
        }

        protected UnitBaseState(UnitController controller)
        {
            Controller = controller;
        }
    }
}
