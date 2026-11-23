using Core;

namespace Sequences
{
    public class StartWorldHandler : Handler
    {
        protected override void OnEnter()
        {
            Game.Level.Deployment.SpawnPlayers(Game.World.Roster.Units);
            Game.World.Start();
            Continue();
        }
    }
}
