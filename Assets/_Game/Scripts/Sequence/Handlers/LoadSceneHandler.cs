using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sequences
{
    /// <summary>
    /// Charge une scène de manière asynchrone, puis passe au handler suivant.
    /// La scène chargée est déposée dans le contexte pour les handlers suivants (ex: l'activer).
    /// </summary>
    public class LoadSceneHandler : Handler<LoadLevelContext>
    {
        private readonly int _buildIndex;
        private readonly LoadSceneMode _mode;

        private AsyncOperation _operation;

        /// <param name="buildIndex">Index de la scène dans les Build Settings.</param>
        /// <param name="mode">Additive par défaut : ne décharge pas la scène courante.</param>
        public LoadSceneHandler(int buildIndex, LoadSceneMode mode = LoadSceneMode.Additive)
        {
            _buildIndex = buildIndex;
            _mode = mode;
        }

        protected override void OnEnter()
        {
            _operation = SceneManager.LoadSceneAsync(_buildIndex, _mode);

            if (_operation == null)
            {
                Debug.LogError(
                    $"[LoadSceneHandler] Chargement de la scène (index {_buildIndex}) impossible. Vérifie les Build Settings.");
                return;
            }

            _operation.completed += OnLoaded;
        }

        protected override void OnExit()
        {
            if (_operation != null)
                _operation.completed -= OnLoaded;

            _operation = null;
        }

        private void OnLoaded(AsyncOperation operation)
        {
            if (Context != null)
                Context.LoadedScene = SceneManager.GetSceneByBuildIndex(_buildIndex);

            Continue();
        }
    }
}
