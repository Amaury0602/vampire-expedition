using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pooling
{
    /// <summary>
    /// Marque une instance comme appartenant à un pool. Ajouté automatiquement par
    /// <see cref="Pool"/> si le prefab ne l'a pas déjà.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        [field: SerializeField] public bool ReleaseOnSceneChange { get; private set; }

        internal GameObject Prefab { get; set; }
        internal Pool OwnerPool { get; set; }

        private CancellationTokenSource _delayedReleaseCts;

        private void OnEnable()
        {
            if (ReleaseOnSceneChange)
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            if (ReleaseOnSceneChange)
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            CancelDelayedRelease();
        }

        private void OnActiveSceneChanged(Scene previous, Scene next)
        {
            Pooler.Release(gameObject);
        }

        internal void StartDelayedRelease(float delay)
        {
            _delayedReleaseCts?.Cancel();
            _delayedReleaseCts?.Dispose();
            _delayedReleaseCts = new CancellationTokenSource();

            DelayedReleaseAsync(delay, _delayedReleaseCts.Token).Forget();
        }

        internal void CancelDelayedRelease()
        {
            _delayedReleaseCts?.Cancel();
            _delayedReleaseCts?.Dispose();
            _delayedReleaseCts = null;
        }

        private async UniTaskVoid DelayedReleaseAsync(float delay, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
                Pooler.Release(gameObject);
            }
            catch (OperationCanceledException) { }
        }
    }
}
