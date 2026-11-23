using Core;
using Worlds;

namespace Sequences
{
    public class InitWorldHandler : Handler
    {
        private readonly WorldSettings _settings;
        private readonly PlayerRosterSettings _roster;

        public InitWorldHandler(WorldSettings settings, PlayerRosterSettings roster)
        {
            _settings = settings;
            _roster = roster;
        }

        protected override void OnEnter()
        {
            Game.InitWorld(_settings, _roster);

            if (Game.World == null)
            {
                return;
            }

            Continue();
        }
    }
}
