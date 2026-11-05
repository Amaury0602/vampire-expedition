using UnityEngine;
using UnityEngine.SceneManagement;
using Sequences;

namespace Game
{
    public class Boot : MonoBehaviour
    {
        private const int GameSceneBuildIndex = 1;
        private const string ColorSortSubSceneName = "ColorSortSubscene";


        private void Start()
        {
            //// Capture directe de la scène Boot : plus fiable que son buildIndex, qui peut ne pas
            //// se résoudre correctement quand le Play est lancé depuis une scène custom
            //// (playModeStartScene / Multiplayer Play Mode scenario) plutôt que via les Build Settings.
            //var bootScene = gameObject.scene;

            //SequenceProvider
            //    .LoadLevelSequence(
            //        GameSceneBuildIndex,
            //        _gameConfig,
            //        unloadScene: bootScene,
            //        subSceneName: ColorSortSubSceneName)
            //    .Start();
        }
    }
}
