using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Battle
{
    public class CombatClock : MonoBehaviour
    {
        [field: SerializeField] public float Speed { get; private set; } = 1f;
        [field: SerializeField] public bool IsPaused { get; private set; }

        public float Scale
        {
            get { return IsPaused ? 0f : Speed; }
        }

        public float DeltaTime { get; private set; }
        public float Time { get; private set; }

        public event Action<float> Ticked;
        public event Action<float> FixedTicked;
        public event Action<float> ScaleChanged;
        public event Action<bool> PauseChanged;

        private float _fixedAccumulator;

        private void Awake()
        {
            Physics2D.simulationMode = SimulationMode2D.Script;
        }

        private void OnDestroy()
        {
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
        }

        private void Update()
        {
            DeltaTime = UnityEngine.Time.deltaTime * Scale;
            Time += DeltaTime;
            Ticked?.Invoke(DeltaTime);
        }

        private void FixedUpdate()
        {
            float step = UnityEngine.Time.fixedDeltaTime;
            _fixedAccumulator += step * Scale;

            while (_fixedAccumulator >= step)
            {
                _fixedAccumulator -= step;
                FixedTicked?.Invoke(step);
                Physics2D.Simulate(step);
            }
        }

        public void Pause()
        {
            SetPaused(true);
        }

        public void Resume()
        {
            SetPaused(false);
        }

        public void TogglePause()
        {
            SetPaused(!IsPaused);
        }

        public void SetSpeed(float speed)
        {
            speed = Mathf.Max(0f, speed);
            if (Mathf.Approximately(Speed, speed))
            {
                return;
            }

            Speed = speed;
            ScaleChanged?.Invoke(Scale);
        }

        public async UniTask Delay(float seconds, CancellationToken token)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                elapsed += DeltaTime;
            }
        }

        private void SetPaused(bool paused)
        {
            if (IsPaused == paused)
            {
                return;
            }

            IsPaused = paused;
            PauseChanged?.Invoke(IsPaused);
            ScaleChanged?.Invoke(Scale);
        }
    }
}
