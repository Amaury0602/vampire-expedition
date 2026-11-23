using Core;

namespace Sequences
{
    public class MoveToNextRoomHandler : Handler<BattleContext>
    {
        protected override void OnEnter()
        {
            Game.Level.Deployment.ClearEnemies();
            Game.Level.Deployment.ClearDeadPlayers();
            Game.World.CompleteRoom(Context.Victory);
            Continue();
        }
    }
}
