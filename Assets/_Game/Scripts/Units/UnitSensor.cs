using System;
using System.Collections.Generic;
using Battle;
using UnityEngine;

namespace Units
{
    public class UnitSensor
    {
        private const float TickInterval = 0.25f;
        private const float SwitchSqrRatio = 0.81f;

        private readonly UnitController _controller;
        private float _timer;

        public UnitController Target { get; private set; }

        public event Action<UnitController> TargetChanged;

        public UnitSensor(UnitController controller)
        {
            _controller = controller;
            _timer = UnityEngine.Random.Range(0f, TickInterval);
        }

        public void Update(float dt)
        {
            _timer -= dt;
            if (_timer > 0f)
            {
                return;
            }

            _timer += TickInterval;
            Scan();
        }

        public void Scan()
        {
            Battlefield battlefield = _controller.Combat.Battlefield;
            if (battlefield == null)
            {
                SetTarget(null);
                return;
            }

            IReadOnlyList<UnitController> enemies = battlefield.GetEnemies(_controller.Combat.Team);
            Vector2 position = _controller.Rigidbody.position;

            UnitController closest = null;
            float closestSqrDistance = float.MaxValue;

            for (int i = 0; i < enemies.Count; i++)
            {
                UnitController enemy = enemies[i];
                if (!UnitCombat.IsValidTarget(enemy))
                {
                    continue;
                }

                float sqrDistance = (enemy.Rigidbody.position - position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closest = enemy;
                }
            }

            if (closest != null && closest != Target && UnitCombat.IsValidTarget(Target))
            {
                float currentSqrDistance = (Target.Rigidbody.position - position).sqrMagnitude;
                if (closestSqrDistance > currentSqrDistance * SwitchSqrRatio)
                {
                    return;
                }
            }

            SetTarget(closest);
        }

        public void Clear()
        {
            SetTarget(null);
            _timer = UnityEngine.Random.Range(0f, TickInterval);
        }

        private void SetTarget(UnitController target)
        {
            if (Target == target)
            {
                return;
            }

            if (Target != null)
            {
                Target.Health.Emptied -= OnTargetEmptied;
            }

            Target = target;

            if (Target != null)
            {
                Target.Health.Emptied += OnTargetEmptied;
            }

            TargetChanged?.Invoke(Target);
        }

        private void OnTargetEmptied()
        {
            SetTarget(null);
            Scan();
        }
    }
}
