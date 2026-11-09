using Damages;
using Pooling;
using Units;
using UnityEngine;

namespace Projectiles
{
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private ProjectileMotion _motion;
        [SerializeField] private Transform _visual;
        [SerializeField] private SpriteRenderer _shadow;

        [Header("Shadow")]
        [SerializeField] private float _shadowMinScale = 0.5f;
        [SerializeField] private float _shadowMinAlpha = 0.3f;

        private UnitController _target;
        private Vector2 _targetPosition;
        private float _speed;
        private float _damage;
        private DamageType _damageTypes;
        private float _knockbackPower;
        private GameObject _source;
        private bool _isFlying;
        private float _traveled;

        private Vector3 _shadowBaseScale;
        private Color _shadowBaseColor;

        private void Reset()
        {
            if (transform.childCount > 0)
            {
                _visual = transform.GetChild(0);
            }
        }

        private void Awake()
        {
            if (_shadow != null)
            {
                _shadowBaseScale = _shadow.transform.localScale;
                _shadowBaseColor = _shadow.color;
            }
        }

        private void Update()
        {
            if (!_isFlying)
            {
                return;
            }

            if (_target != null && _target.isActiveAndEnabled)
            {
                _targetPosition = _target.Hitbox.Center;
            }

            Vector2 position = transform.position;
            Vector2 offset = _targetPosition - position;
            float step = _speed * Time.deltaTime;

            if (offset.sqrMagnitude <= step * step)
            {
                transform.position = _targetPosition;
                Impact(offset);
                return;
            }

            Vector2 direction = offset.normalized;
            Vector2 newPosition = position + direction * step;
            _traveled += step;

            float remaining = (_targetPosition - newPosition).magnitude;
            float t = _traveled / (_traveled + remaining);

            Vector3 previousVisualPosition = GetVisualPosition();
            transform.position = newPosition;
            ApplyHeight(t);
            Orient(direction, previousVisualPosition);
        }

        public void Launch(UnitController target, float speed, float damage, DamageType damageTypes, float knockbackPower, GameObject source)
        {
            _target = target;
            _targetPosition = target.Hitbox.Center;
            _speed = speed;
            _damage = damage;
            _damageTypes = damageTypes;
            _knockbackPower = knockbackPower;
            _source = source;
            _traveled = 0f;
            _isFlying = true;

            ApplyHeight(0f);

            Vector2 direction = (_targetPosition - (Vector2)transform.position).normalized;
            Vector2 launchDirection = direction;
            if (_motion != null)
            {
                float distance = (_targetPosition - (Vector2)transform.position).magnitude;
                float startHeight = _motion.EvaluateHeight(0.05f);
                launchDirection = direction * distance * 0.05f + Vector2.up * startHeight;
            }

            SetVisualRotation(launchDirection);
            SetShadowRotation(direction);
        }

        public void OnGet()
        {
        }

        public void OnRelease()
        {
            _isFlying = false;
            _target = null;
            _source = null;
            ApplyHeight(0f);
        }

        private void Impact(Vector2 direction)
        {
            _isFlying = false;

            if (Targeting.IsValidTarget(_target))
            {
                DamageInfo damage = new DamageInfo(_damage, direction.normalized, _damageTypes, _knockbackPower, _source);
                _target.Health.TakeDamage(damage);
            }

            Pooler.Release(gameObject);
        }

        private void ApplyHeight(float t)
        {
            float height = _motion != null ? _motion.EvaluateHeight(t) : 0f;

            if (_visual != null)
            {
                _visual.localPosition = Vector3.up * height;
            }

            UpdateShadow(height);
        }

        private void UpdateShadow(float height)
        {
            if (_shadow == null)
            {
                return;
            }

            float ratio = _motion != null && _motion.Height > 0f ? Mathf.Clamp01(height / _motion.Height) : 0f;

            _shadow.transform.localScale = _shadowBaseScale * Mathf.Lerp(1f, _shadowMinScale, ratio);

            Color color = _shadowBaseColor;
            color.a = _shadowBaseColor.a * Mathf.Lerp(1f, _shadowMinAlpha, ratio);
            _shadow.color = color;
        }

        private Vector3 GetVisualPosition()
        {
            if (_visual != null)
            {
                return _visual.position;
            }

            return transform.position;
        }

        private void Orient(Vector2 groundDirection, Vector3 previousVisualPosition)
        {
            Vector2 visualDirection = (Vector2)(GetVisualPosition() - previousVisualPosition);
            if (visualDirection.sqrMagnitude > 0.000001f)
            {
                SetVisualRotation(visualDirection);
            }

            SetShadowRotation(groundDirection);
        }

        private void SetVisualRotation(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                return;
            }

            Transform target = _visual != null ? _visual : transform;
            target.right = direction;
        }

        private void SetShadowRotation(Vector2 direction)
        {
            if (_shadow == null || direction == Vector2.zero)
            {
                return;
            }

            _shadow.transform.right = direction;
        }
    }
}
