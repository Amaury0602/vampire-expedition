using Core;

namespace Sequences
{
    public class InitGameHandler : Handler
    {
        protected override void OnEnter()
        {
            Game.Init();

            if (!Game.IsInitialized)
                return;

            Continue();
        }
    }
}
