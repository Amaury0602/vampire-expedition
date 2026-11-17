using System;
using Battle;
using Core;
using Pooling;
using UnityEngine;

namespace Skills
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SkillFx : MonoBehaviour, IPoolable
    {
        [SerializeField] private SkillFxLevelVariant[] _variants = Array.Empty<SkillFxLevelVariant>();

        public int Level { get; private set; }

        private ParticleSystem[] _particles;
        private Animator[] _animators;
        private CombatClock _clock;
        private bool _playing;

        private void Awake()
        {
            ParticleSystem.MainModule main = GetComponent<ParticleSystem>().main;
            main.stopAction = ParticleSystemStopAction.Callback;
            _particles = GetComponentsInChildren<ParticleSystem>(true);
            _animators = GetComponentsInChildren<Animator>(true);
        }

        private void OnDestroy()
        {
            _playing = false;
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
            _playing = true;
        }

        private void OnParticleSystemStopped()
        {
            if (!_playing)
            {
                return;
            }

            _playing = false;
            Pooler.Release(gameObject);
        }

        public void OnGet()
        {
        }

        public void OnRelease()
        {
            _playing = false;
            SetClock(null);
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
    }
}
