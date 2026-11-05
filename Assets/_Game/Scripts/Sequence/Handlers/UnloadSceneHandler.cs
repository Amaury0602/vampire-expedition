using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sequences
{
    /// <summary>
    /// Décharge une scène de manière asynchrone, puis passe au handler suivant.
    /// </summary>
    public class UnloadSceneHandler : Handler
    {
        private readonly int _buildIndex;
        private readonly Scene? _explicitScene;

        private AsyncOperation _operation;

        /// <param name="buildIndex">Index de la scène à décharger dans les Build Settings.</param>
        public UnloadSceneHandler(int buildIndex)
        {
            _buildIndex = buildIndex;
        }

        /// <param name="scene">Scène à décharger, capturée directement (fiable même hors flux Build Settings standard,
        /// ex : Play lancé depuis une scène custom où le buildIndex ne se résout pas correctement).</param>
        public UnloadSceneHandler(Scene scene)
        {
            _explicitScene = scene;
            _buildIndex = scene.buildIndex;
        }

        protected override void OnEnter()
        {
            Scene scene = _explicitScene ?? SceneManager.GetSceneByBuildIndex(_buildIndex);

            Debug.Log("unload scene");

            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogWarning($"[UnloadSceneHandler] Scène (index {_buildIndex}) non chargée — étape sautée.");
                Continue();
                return;
            }

            _operation = SceneManager.UnloadSceneAsync(scene);

            if (_operation == null)
            {
                Debug.LogError($"[UnloadSceneHandler] Déchargement de la scène (index {_buildIndex}) impossible.");
                return;
            }

            _operation.completed += OnUnloaded;
        }

        protected override void OnExit()
        {
            if (_operation != null)
                _operation.completed -= OnUnloaded;

            _operation = null;
        }

        private void OnUnloaded(AsyncOperation operation)
        {

            Debug.Log($"scene {_buildIndex} unloaded");

            Continue();
        }
    }
}
