using System;

namespace Player
{
    public class BaseState
    {
        public event Action Entered;
        public event Action Updated;
        public event Action FixedUpdated;
        public event Action Exited;

        public virtual void EnterState(StateParameters parameters = null) => Entered?.Invoke();
        public virtual void UpdateState(float dt) => Updated?.Invoke();
        public virtual void FixedUpdateState(float dt) => FixedUpdated?.Invoke();
        public virtual void ExitState() => Exited?.Invoke();

        public void ClearEvents()
        {
            Entered = null;
            Updated = null;
            FixedUpdated = null;
            Exited = null;
        }
    }
}