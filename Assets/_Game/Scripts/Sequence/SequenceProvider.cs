using Menus;
using UnityEngine;
using UnityEngine.SceneManagement;
using Worlds;

namespace Sequences
{
    public static class SequenceProvider
    {
        public static Sequence BootSequence(MenuDatabase menuDatabase, Transform menuRoot, int gameSceneBuildIndex, WorldSettings world, PlayerRosterSettings roster)
        {
            Sequence sequence = new Sequence(new LoadLevelContext())
                .Add(new InitMenusHandler(menuDatabase, menuRoot))
                .Add(new LoadSceneHandler(gameSceneBuildIndex))
                .Add(new SetActiveSceneHandler())
                .Add(new InitGameHandler())
                .Add(new InitWorldHandler(world, roster))
                .Add(new InitPoolsHandler())
                .Add(new StartWorldHandler());

            return sequence;
        }

        public static Sequence BattleRoomSequence(BattleRoom room)
        {
            Sequence sequence = new Sequence(new BattleContext { Room = room })
                .Add(new PrepareBattleHandler())
                .Add(new StartBattleHandler())
                .Add(new MoveToNextRoomHandler());

            return sequence;
        }
    }

    public class LoadLevelContext : SequenceContext
    {
        public Scene LoadedScene;
    }

    public class BattleContext : SequenceContext
    {
        public BattleRoom Room;
        public bool Victory;
    }
}
