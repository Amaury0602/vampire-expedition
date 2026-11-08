using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Units
{
    public class UnitSeparation
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly NavigationSettings _navigationSettings;
        private readonly MovementSettings _movementSettings;
        private readonly List<Collider2D> _neighbours = new List<Collider2D>();
        private ContactFilter2D _filter;

        public UnitSeparation(Rigidbody2D rigidbody, NavigationSettings navigationSettings, MovementSettings movementSettings)
        {
            _rigidbody = rigidbody;
            _navigationSettings = navigationSettings;
            _movementSettings = movementSettings;

            _filter = new ContactFilter2D();
            _filter.SetLayerMask(navigationSettings.SeparationLayers);
            _filter.useTriggers = false;
        }

        public Vector2 Compute()
        {
            float radius = _navigationSettings.SeparationRadius;
            if (radius <= 0f)
                return Vector2.zero;

            Vector2 position = _rigidbody.position;
            Vector2 separation = Vector2.zero;

            Physics2D.OverlapCircle(position, radius, _filter, _neighbours);

            for (int i = 0; i < _neighbours.Count; i++)
            {
                Rigidbody2D other = _neighbours[i].attachedRigidbody;
                if (other == null || other == _rigidbody || other.bodyType != RigidbodyType2D.Dynamic)
                    continue;

                Vector2 offset = position - other.position;
                float distance = offset.magnitude;
                if (distance >= radius)
                    continue;

                Vector2 direction = distance > 0.0001f ? offset / distance : Random.insideUnitCircle.normalized;
                separation += direction * (1f - distance / radius);
            }

            return separation * _movementSettings.MaxSpeed * _navigationSettings.SeparationWeight;
        }
    }
}
