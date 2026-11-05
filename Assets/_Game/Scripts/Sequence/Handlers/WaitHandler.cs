using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Sequences
{
    /// <summary>
    /// Attend une durée en secondes puis passe au handler suivant.
    /// L'attente est annulée si la séquence est stoppée (via OnExit).
    /// </summary>
    public class WaitHandler : Handler
    {
        private readonly float _duration;
        private readonly bool _ignoreTimeScale;

        private CancellationTokenSource _cts;

        /// <param name="durationSeconds">Durée de l'attente, en secondes.</param>
        /// <param name="ignoreTimeScale">Si vrai, l'attente ignore Time.timeScale (menus, pause).</param>
        public WaitHandler(float durationSeconds, bool ignoreTimeScale = false)
        {
            _duration = Mathf.Max(0f, durationSeconds);
            _ignoreTimeScale = ignoreTimeScale;
        }

        protected override void OnEnter()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            WaitAsync(_cts.Token).Forget();
        }

        protected override void OnExit()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTaskVoid WaitAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_duration),
                    _ignoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime,
                    cancellationToken: token);

                Continue();
            }
            catch (OperationCanceledException) { }
        }
    }
}
