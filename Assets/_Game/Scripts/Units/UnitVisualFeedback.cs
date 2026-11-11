using Battle;
using Damages;
using PrimeTween;
using UnityEngine;

namespace Units
{
    public class UnitVisualFeedback : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private SpriteRenderer[] _renderers;

        [Header("Blink")]
        [SerializeField] private Color _blinkColor = Color.white;
        [SerializeField] private float _blinkDuration = 0.06f;
        [SerializeField] private int _blinkCount = 2;

        private Color[] _baseColors;
        private Tween _tween;
        private UnitController _controller;
        private CombatClock _clock;

        private void Reset()
        {
            _health = GetComponentInParent<Health>();
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void Awake()
        {
            _controller = GetComponentInParent<UnitController>();

            _baseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _baseColors[i] = _renderers[i].color;
            }
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;

            if (_controller != null)
            {
                _controller.ClockChanged += OnClockChanged;
                OnClockChanged(_controller.Clock);
            }
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;

            if (_controller != null)
            {
                _controller.ClockChanged -= OnClockChanged;
            }

            OnClockChanged(null);
            StopFeedback();
        }

        private void OnClockChanged(CombatClock clock)
        {
            if (_clock != null)
            {
                _clock.ScaleChanged -= OnScaleChanged;
            }

            _clock = clock;

            if (_clock != null)
            {
                _clock.ScaleChanged += OnScaleChanged;
            }

            ApplyTimeScale();
        }

        private void OnScaleChanged(float scale)
        {
            ApplyTimeScale();
        }

        private void ApplyTimeScale()
        {
            if (_tween.isAlive)
            {
                _tween.timeScale = _clock != null ? _clock.Scale : 1f;
            }
        }

        private void OnDamaged(DamageInfo damage)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            PlayHitFeedback();
        }

        public void PlayHitFeedback()
        {
            StopFeedback();

            int cycles = Mathf.Max(1, _blinkCount) * 2;
            _tween = Tween.Custom(this, 0f, 1f, _blinkDuration, (target, value) => { target.ApplyBlink(value); }, Ease.Linear, cycles, CycleMode.Yoyo);
            ApplyTimeScale();
        }

        private void ApplyBlink(float value)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].color = Color.LerpUnclamped(_baseColors[i], _blinkColor, value);
            }
        }

        private void StopFeedback()
        {
            if (_tween.isAlive)
            {
                _tween.Stop();
            }

            ApplyBlink(0f);
        }
    }
}
