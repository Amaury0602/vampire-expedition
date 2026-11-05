using System;
using UnityEngine;

namespace Sequences
{
    /// <summary>
    /// Handler instantané : exécute une action puis enchaîne immédiatement sur le handler suivant.
    /// Exemple canonique du handler qui appelle Continue() depuis OnEnter().
    /// </summary>
    public class ActionHandler : Handler
    {
        private readonly Action _action;
        private readonly Action<SequenceContext> _contextAction;

        /// <param name="action">Action exécutée à l'entrée du handler.</param>
        public ActionHandler(Action action)
        {
            _action = action;
        }

        /// <param name="action">Action recevant le contexte partagé de la séquence.</param>
        public ActionHandler(Action<SequenceContext> action)
        {
            _contextAction = action;
        }

        protected override void OnEnter()
        {
            if (_action == null && _contextAction == null)
                Debug.LogWarning("[ActionHandler] Aucune action fournie — étape sans effet.");

            _action?.Invoke();
            _contextAction?.Invoke(Context);


            Debug.Log("action handler ended");

            Continue();
        }
    }
}
