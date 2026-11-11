using Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sequences
{
    public static class SequenceProvider
    {
        public static Sequence BootSequence(MenuDatabase menuDatabase, Transform menuRoot, int gameSceneBuildIndex)
        {
            Sequence sequence = new Sequence(new LoadLevelContext())
                .Add(new InitMenusHandler(menuDatabase, menuRoot))
                .Add(new LoadSceneHandler(gameSceneBuildIndex))
                .Add(new SetActiveSceneHandler())
                .Add(new InitGameHandler());

            return sequence;
        }
    }

    public class LoadLevelContext : SequenceContext
    {
        public Scene LoadedScene;
    }
}
