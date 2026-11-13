using Battle;
using Damages;
using PrimeTween;
using UnityEngine;

namespace Units
{
    public class UnitHealthBar : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private SpriteRenderer _bar;

        [Header("Visibility")]
        [SerializeField, Min(0f)] private float _hideDelay = 2f;

        [Header("Ghost")]
        [SerializeField, Min(0f)] private float _ghostDelay = 0.2f;
        [SerializeField, Min(0f)] private float _ghostDuration = 0.3f;
        [SerializeField] private Ease _ghostEase = Ease.OutQuad;

        private UnitController _controller;
        private CombatClock _clock;
        private Tween _hideTween;
        private Tween _ghostTween;
        private float _fill;
        private float _ghost;

        private void Reset()
        {
            _health = GetComponentInParent<Health>();
            _bar = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            _controller = GetComponentInParent<UnitController>();
        }

        private void OnEnable()
        {
            _health.Changed += OnHealthChanged;
            _health.Damaged += OnDamaged;
            _health.Emptied += OnEmptied;

            if (_controller != null)
            {
                _controller.ClockChanged += OnClockChanged;
                OnClockChanged(_controller.Clock);
            }

            Hide();
        }

        private void OnDisable()
        {
            _health.Changed -= OnHealthChanged;
            _health.Damaged -= OnDamaged;
            _health.Emptied -= OnEmptied;

            if (_controller != null)
            {
                _controller.ClockChanged -= OnClockChanged;
            }

            OnClockChanged(null);
            Hide();
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
            float scale = _clock != null ? _clock.Scale : 1f;

            if (_hideTween.isAlive)
            {
                _hideTween.timeScale = scale;
            }

            if (_ghostTween.isAlive)
            {
                _ghostTween.timeScale = scale;
            }
        }

        private void OnHealthChanged(float current, float max)
        {
            _fill = GetRatio();
            StopTween(ref _ghostTween);

            if (_fill >= _ghost || !isActiveAndEnabled)
            {
                _ghost = _fill;
                ApplyColor();
                return;
            }

            ApplyColor();
            _ghostTween = Tween.Custom(this, _ghost, _fill, _ghostDuration, (bar, value) => bar.SetGhost(value), _ghostEase, startDelay: _ghostDelay);
            ApplyTimeScale();
        }

        private void OnDamaged(DamageInfo damage)
        {
            if (_health.IsEmpty || !isActiveAndEnabled)
            {
                return;
            }

            _bar.enabled = true;

            StopTween(ref _hideTween);
            _hideTween = Tween.Delay(this, _hideDelay, bar => bar.Hide());
            ApplyTimeScale();
        }

        private void OnEmptied()
        {
            Hide();
        }

        private void Hide()
        {
            StopTween(ref _hideTween);
            StopTween(ref _ghostTween);

            _fill = GetRatio();
            _ghost = _fill;
            ApplyColor();

            _bar.enabled = false;
        }

        private void SetGhost(float value)
        {
            _ghost = value;
            ApplyColor();
        }

        private void ApplyColor()
        {
            float team = _controller != null && _controller.Combat.Team != Team.Player ? 1f : 0f;
            _bar.color = new Color(_fill, _ghost, team, 1f);
        }

        private float GetRatio()
        {
            return _health.Max > 0f ? Mathf.Clamp01(_health.Current / _health.Max) : 0f;
        }

        private static void StopTween(ref Tween tween)
        {
            if (tween.isAlive)
            {
                tween.Stop();
            }
        }
    }
}
