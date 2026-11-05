using UnityEngine.SceneManagement;

namespace Sequences
{
    public static class SequenceProvider
    {
        //public static Sequence LoadLevelSequence(
        //    int buildIndex,
        //    Game.GameConfig config,
        //    Scene? unloadScene = null,
        //    string subSceneName = null)
        //{
        //    Sequence sequence = new Sequence(new LoadLevelContext())
        //        .Add(new LoadSceneHandler(buildIndex))
        //        .Add(new SetActiveSceneHandler())
        //        .Add(new LoadSubSceneHandler(subSceneName))
        //        .Add(new ActionHandler(() => GameStatic.Init(config)))
        //        .Add(new UnloadSceneHandler(unloadScene.Value))
        //        .Add(new InitEntitiesWorld());

        //    return sequence;
        //}
    }

    public class LoadLevelContext : SequenceContext
    {
        public Scene LoadedScene;
    }
}
