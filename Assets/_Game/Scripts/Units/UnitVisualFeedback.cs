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

        private void Reset()
        {
            _health = GetComponentInParent<Health>();
            _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        private void Awake()
        {
            _baseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _baseColors[i] = _renderers[i].color;
            }
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            StopFeedback();
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
