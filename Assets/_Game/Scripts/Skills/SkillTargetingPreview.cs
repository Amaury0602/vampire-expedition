using Units;
using UnityEngine;

namespace Skills
{
    public class SkillTargetingPreview : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Color _validColor = new Color(0.3f, 1f, 0.4f, 0.35f);
        [SerializeField] private Color _invalidColor = new Color(1f, 0.3f, 0.3f, 0.35f);
        [SerializeField] private float _unitMargin = 0.15f;
        [SerializeField] private float _cursorRadius = 0.2f;

        private void Reset()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Awake()
        {
            Hide();
        }

        public void ShowArea(Vector2 position, float radius, bool valid)
        {
            Show(position, radius, valid);
        }

        public void ShowUnit(UnitController unit, bool valid)
        {
            Show(unit.Hitbox.Center, unit.Hitbox.Radius + _unitMargin, valid);
        }

        public void ShowCursor(Vector2 position)
        {
            Show(position, _cursorRadius, false);
        }

        public void Hide()
        {
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
        }

        private void Show(Vector2 position, float radius, bool valid)
        {
            if (_renderer == null)
            {
                return;
            }

            transform.position = new Vector3(position.x, position.y, transform.position.z);
            transform.localScale = Vector3.one * GetScale(radius);
            _renderer.color = valid ? _validColor : _invalidColor;
            _renderer.enabled = true;
        }

        private float GetScale(float radius)
        {
            float diameter = 1f;
            if (_renderer.sprite != null)
            {
                diameter = _renderer.sprite.bounds.size.x;
            }

            if (diameter <= 0f)
            {
                return radius * 2f;
            }

            return radius * 2f / diameter;
        }
    }
}
