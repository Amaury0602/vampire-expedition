using Battle;
using Damages;
using PrimeTween;
using UnityEngine;

namespace Units
{
    public class UnitVisualFeedback : MonoBehaviour
    {
        private static readonly int FLASH_STEP_PROPERTY = Shader.PropertyToID("_FlashStep");

        [SerializeField] private Health _health;
        [SerializeField] private SpriteRenderer _renderer;

        [Header("Blink")]
        [SerializeField] private float _blinkDuration = 0.06f;
        private Material _materialInstance;
        private Tween _tween;
        private UnitController _controller;
        private CombatClock _clock;

        private void Reset()
        {
            _health = GetComponentInParent<Health>();
            _renderer = GetComponentInChildren<SpriteRenderer>(true);
        }

        private void Awake()
        {
            _controller = GetComponentInParent<UnitController>();

            if (_renderer == null)
            {
                Debug.LogWarning($"[UnitVisualFeedback] '{name}' has no flash renderer.", this);
                return;
            }

            _materialInstance = _renderer.material;
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
            {
                Destroy(_materialInstance);
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
            _tween = Tween.Custom(this, 0f, 0.75f, _blinkDuration, (target, value) => { target.ApplyBlink(value); }, Ease.Linear, 2, CycleMode.Yoyo);
            ApplyTimeScale();
        }

        private void ApplyBlink(float value)
        {
            if (_materialInstance != null)
            {
                _materialInstance.SetFloat(FLASH_STEP_PROPERTY, value);
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
