using Battle;
using PrimeTween;
using UnityEngine;

namespace Units
{
    public class UnitFacing : MonoBehaviour
    {
        [SerializeField] private UnitController _controller;
        [SerializeField] private float _flipDuration = 0.15f;
        [SerializeField] private Ease _flipEase = Ease.OutBack;

        private float _baseScaleX;
        private Tween _tween;
        private CombatClock _clock;

        private void Reset()
        {
            _controller = GetComponentInParent<UnitController>();
        }

        private void Awake()
        {
            _baseScaleX = Mathf.Abs(transform.localScale.x);
        }

        private void OnEnable()
        {
            _controller.FacingChanged += OnFacingChanged;
            _controller.ClockChanged += OnClockChanged;
            OnClockChanged(_controller.Clock);
            Snap();
        }

        private void Start()
        {
            Snap();
        }

        private void OnDisable()
        {
            _controller.FacingChanged -= OnFacingChanged;
            _controller.ClockChanged -= OnClockChanged;
            OnClockChanged(null);
            StopTween();
        }

        private void OnFacingChanged(int facing)
        {
            StopTween();
            _tween = Tween.ScaleX(transform, facing * _baseScaleX, _flipDuration, _flipEase);
            ApplyTimeScale();
        }

        private void Snap()
        {
            StopTween();
            Vector3 scale = transform.localScale;
            scale.x = _controller.Facing * _baseScaleX;
            transform.localScale = scale;
        }

        private void StopTween()
        {
            if (_tween.isAlive)
            {
                _tween.Stop();
            }
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
    }
}
