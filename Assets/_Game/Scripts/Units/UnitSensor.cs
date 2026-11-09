using System;

namespace Units
{
    public class UnitSensor
    {
        private const float TickInterval = 0.25f;

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
            SetTarget(Targeting.FindTarget(_controller, Target));
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
