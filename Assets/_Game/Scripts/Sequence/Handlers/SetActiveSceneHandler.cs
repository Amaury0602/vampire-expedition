using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sequences
{
    /// <summary>
    /// Active la scène déposée dans le contexte par un LoadSceneHandler précédent.
    /// Étape instantanée.
    /// </summary>
    public class SetActiveSceneHandler : Handler<LoadLevelContext>
    {
        protected override void OnEnter()
        {
            if (Context != null && Context.LoadedScene.IsValid())
                SceneManager.SetActiveScene(Context.LoadedScene);
            else
                Debug.LogWarning("[SetActiveSceneHandler] Aucune scène chargée dans le contexte — étape sans effet.");

            Continue();
        }
    }
}
