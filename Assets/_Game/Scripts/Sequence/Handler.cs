using System;
using UnityEngine;

namespace Sequences
{
    /// <summary>
    /// Étape d'une <see cref="Sequence"/>. Dériver et implémenter OnEnter().
    /// Appeler Continue() pour passer au handler suivant — y compris directement
    /// depuis OnEnter() pour une étape instantanée.
    /// </summary>
    public abstract class Handler
    {
        /// <summary> Contexte partagé fourni par la séquence. Null hors exécution. </summary>
        protected SequenceContext Context { get; private set; }

        /// <summary> Vrai entre OnEnter() et OnExit(). </summary>
        protected bool IsActive { get; private set; }

        // Plomberie : seule la Sequence s'abonne à cet event et appelle Enter/Exit.
        internal event Action<Handler> ContinueRequested;

        internal virtual void Enter(SequenceContext context)
        {
            Context = context;
            IsActive = true;
            OnEnter();
        }

        internal void Exit()
        {
            // IsActive passe à false AVANT OnExit() : un Continue() depuis OnExit() est ainsi rejeté.
            IsActive = false;
            OnExit();
            Context = null;
        }

        /// <summary> Début de l'étape. Appeler Continue() ici pour une étape instantanée. </summary>
        protected abstract void OnEnter();

        /// <summary>
        /// Nettoyage de l'étape. Appelé après Continue(), et aussi lorsque la séquence est stoppée.
        /// C'est ici qu'on annule les tâches en cours et qu'on se désabonne des events.
        /// </summary>
        protected virtual void OnExit() { }

        /// <summary> Signale à la séquence que cette étape est terminée. </summary>
        protected void Continue()
        {
            if (!IsActive)
            {
                Debug.LogWarning($"[{GetType().Name}] Continue() ignoré : le handler n'est pas actif.");
                return;
            }

            ContinueRequested?.Invoke(this);
        }
    }

    /// <summary>
    /// Variante typée : évite de caster le contexte dans chaque handler.
    /// </summary>
    /// <typeparam name="TContext">Type concret de contexte attendu par ce handler.</typeparam>
    public abstract class Handler<TContext> : Handler where TContext : SequenceContext
    {
        /// <summary> Contexte déjà casté dans le type attendu. Null si le type ne correspond pas. </summary>
        protected new TContext Context => base.Context as TContext;

        internal override void Enter(SequenceContext context)
        {
            if (context != null && context is not TContext)
            {
                Debug.LogWarning(
                    $"[{GetType().Name}] Contexte de type '{context.GetType().Name}' incompatible " +
                    $"avec '{typeof(TContext).Name}' — Context sera null.");
            }

            base.Enter(context);
        }
    }
}
