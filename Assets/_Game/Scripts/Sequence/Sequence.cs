using System;
using System.Collections.Generic;
using UnityEngine;

// Note : PrimeTween expose aussi un type nommé 'Sequence'. Si un fichier importe les deux
// namespaces (using PrimeTween; using Sequences;), lever l'ambiguïté CS0104 avec :
//     using Sequence = Sequences.Sequence;

namespace Sequences
{
    /// <summary>
    /// Enchaîne des <see cref="Handler"/> dans l'ordre. Chaque handler signale la fin de son
    /// travail via Continue() ; la séquence passe alors au suivant.
    /// Classe C# pure : aucun MonoBehaviour, aucun Update par frame.
    /// </summary>
    public class Sequence
    {
        /// <summary> Contexte partagé transmis à chaque handler (écrit par l'un, lu par l'autre). </summary>
        public SequenceContext Context { get; }

        /// <summary> Vrai entre Start() et la fin de la séquence (ou Stop()). </summary>
        public bool IsRunning => _isRunning;

        /// <summary> Handler actuellement actif. Null si la séquence ne tourne pas. </summary>
        public Handler CurrentHandler => _currentHandler;

        /// <summary> Émis quand le dernier handler a terminé. </summary>
        public event Action Completed;

        /// <summary> Émis quand Stop() interrompt la séquence. </summary>
        public event Action Stopped;

        private readonly List<Handler> _handlers = new();

        private Handler _currentHandler;
        private int _index = -1;
        private bool _isRunning;

        // --- Trampoline ---
        // Un handler peut appeler Continue() dès son OnEnter(). Sans ces deux drapeaux, la séquence
        // se rappellerait récursivement (pile qui enfle, index corrompu). Ici la demande est mise
        // en attente et c'est la boucle de Pump() déjà en cours qui la consomme.
        private bool _isPumping;
        private bool _advanceRequested;

        /// <param name="context">Contexte partagé. Si null, un SequenceContext vide est créé.</param>
        public Sequence(SequenceContext context = null)
        {
            Context = context ?? new SequenceContext();
        }

        /// <summary> Raccourci de construction : contexte + handlers dans l'ordre. </summary>
        public Sequence(SequenceContext context, params Handler[] handlers) : this(context)
        {
            if (handlers == null)
                return;

            for (int i = 0; i < handlers.Length; i++)
                Add(handlers[i]);
        }

        /// <summary> Ajoute un handler à la fin de la chaîne. Chaînable. </summary>
        public Sequence Add(Handler handler)
        {
            if (handler == null)
            {
                Debug.LogWarning("[Sequence] Add() a reçu un handler null — ignoré.");
                return this;
            }

            if (_isRunning)
            {
                Debug.LogWarning($"[Sequence] Add('{handler.GetType().Name}') pendant l'exécution — ignoré.");
                return this;
            }

            _handlers.Add(handler);
            return this;
        }

        /// <summary> Démarre la séquence au premier handler. No-op si elle tourne déjà. </summary>
        public void Start()
        {
            if (_isRunning)
            {
                Debug.LogWarning("[Sequence] Start() appelé alors que la séquence tourne déjà — ignoré.");
                return;
            }

            if (_handlers.Count == 0)
            {
                Debug.LogWarning("[Sequence] Start() sur une séquence vide — rien à exécuter.");
                return;
            }

            _isRunning = true;
            _index = -1;
            _currentHandler = null;

            _advanceRequested = true;
            Pump();
        }

        /// <summary>
        /// Interrompt la séquence. OnExit() est appelé sur le handler courant.
        /// Sûr depuis l'intérieur d'un OnEnter() : la pompe s'arrête proprement.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning)
            {
                Debug.LogWarning("[Sequence] Stop() appelé alors qu'aucune séquence ne tourne — ignoré.");
                return;
            }

            _isRunning = false;
            _advanceRequested = false;
            _index = -1;

            ExitCurrent();

            Stopped?.Invoke();
        }

        // Boucle de progression. Un seul niveau de pompe est actif à la fois : toute demande émise
        // pendant un OnEnter() est consommée par la boucle en cours, jamais par récursion.
        private void Pump()
        {
            if (_isPumping)
                return;

            _isPumping = true;

            try
            {
                while (_isRunning && _advanceRequested)
                {
                    _advanceRequested = false;

                    ExitCurrent();

                    // Un OnExit() a pu appeler Stop() : la condition de boucle tranchera.
                    if (!_isRunning)
                        continue;

                    _index++;

                    if (_index >= _handlers.Count)
                    {
                        Complete();
                        // 'continue' et non 'break' : un listener de Completed peut relancer Start().
                        continue;
                    }

                    EnterCurrent();
                }
            }
            finally
            {
                _isPumping = false;
            }
        }

        private void EnterCurrent()
        {
            Handler handler = _handlers[_index];

            if (handler == null)
            {
                Debug.LogWarning($"[Sequence] Handler null à l'index {_index} — étape sautée.");
                _advanceRequested = true;
                return;
            }

            _currentHandler = handler;
            handler.ContinueRequested += OnHandlerContinue;
            handler.Enter(Context);
        }

        // Idempotent : appelable depuis Stop() puis depuis la pompe sans double OnExit().
        private void ExitCurrent()
        {
            if (_currentHandler == null)
                return;

            Handler handler = _currentHandler;
            _currentHandler = null;

            handler.ContinueRequested -= OnHandlerContinue;
            handler.Exit();
        }

        private void Complete()
        {
            _isRunning = false;
            _advanceRequested = false;
            _index = -1;
            _currentHandler = null; // déjà sorti par ExitCurrent()

            Completed?.Invoke();
        }

        private void OnHandlerContinue(Handler sender)
        {
            if (!_isRunning)
            {
                Debug.LogWarning(
                    $"[Sequence] Continue() de '{sender.GetType().Name}' ignoré : séquence terminée ou stoppée.");
                return;
            }

            // Filet anti-handler périmé, au cas où un désabonnement aurait été raté.
            if (sender != _currentHandler)
            {
                Debug.LogWarning(
                    $"[Sequence] Continue() de '{sender.GetType().Name}' ignoré : ce n'est plus le handler courant.");
                return;
            }

            if (_advanceRequested)
            {
                Debug.LogWarning(
                    $"[Sequence] Continue() appelé plusieurs fois par '{sender.GetType().Name}' — appel ignoré.");
                return;
            }

            _advanceRequested = true;
            Pump();
        }
    }

    /// <summary>
    /// Contexte partagé d'une séquence. Classe de base vide, à dériver pour transporter
    /// des données d'un handler à l'autre (même idiome que StateParameters).
    /// </summary>
    public class SequenceContext { }
}
