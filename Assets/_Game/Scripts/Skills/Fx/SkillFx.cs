using System;
using System.Threading;
using Battle;
using Core;
using Cysharp.Threading.Tasks;
using Pooling;
using UnityEngine;

namespace Skills
{
    public class SkillFx : MonoBehaviour, IPoolable
    {
        [SerializeField, Min(0f)] private float _duration = 1f;
        [SerializeField] private SkillFxLevelVariant[] _variants = Array.Empty<SkillFxLevelVariant>();

        public int Level { get; private set; }

        private ParticleSystem[] _particles;
        private Animator[] _animators;
        private CombatClock _clock;
        private CancellationTokenSource _releaseCts;

        private void Awake()
        {
            _particles = GetComponentsInChildren<ParticleSystem>(true);
            _animators = GetComponentsInChildren<Animator>(true);
        }

        private void OnDestroy()
        {
            CancelRelease();
            SetClock(null);
        }

        public void Play(Vector2 position, float scale, int level)
        {
            Level = level;
            transform.position = position;
            transform.localScale = Vector3.one * scale;

            ApplyVariants(level);
            SetClock(Game.Level != null ? Game.Level.Clock : null);
            ApplyTimeScale(_clock != null ? _clock.Scale : 1f);
            PlayParticles();

            CancelRelease();
            _releaseCts = new CancellationTokenSource();
            ReleaseAsync(_releaseCts.Token).Forget();
        }

        public void OnGet()
        {
        }

        public void OnRelease()
        {
            CancelRelease();
            SetClock(null);
            StopParticles();
            Level = 0;
        }

        private void ApplyVariants(int level)
        {
            for (int i = 0; i < _variants.Length; i++)
            {
                SkillFxLevelVariant variant = _variants[i];
                if (variant != null && variant.Target != null)
                {
                    variant.Target.SetActive(variant.Matches(level));
                }
            }
        }

        private void PlayParticles()
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                ParticleSystem particles = _particles[i];
                particles.Clear(false);
                if (particles.gameObject.activeInHierarchy)
                {
                    particles.Play(false);
                }
            }
        }

        private void StopParticles()
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                _particles[i].Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void SetClock(CombatClock clock)
        {
            if (_clock != null)
            {
                _clock.ScaleChanged -= ApplyTimeScale;
            }

            _clock = clock;

            if (_clock != null)
            {
                _clock.ScaleChanged += ApplyTimeScale;
            }
        }

        private void ApplyTimeScale(float scale)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                ParticleSystem.MainModule main = _particles[i].main;
                main.simulationSpeed = scale;
            }

            for (int i = 0; i < _animators.Length; i++)
            {
                _animators[i].speed = scale;
            }
        }

        private async UniTaskVoid ReleaseAsync(CancellationToken token)
        {
            try
            {
                if (_clock != null)
                {
                    await _clock.Delay(_duration, token);
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_duration), cancellationToken: token);
                }

                Pooler.Release(gameObject);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CancelRelease()
        {
            _releaseCts?.Cancel();
            _releaseCts?.Dispose();
            _releaseCts = null;
        }
    }
}
