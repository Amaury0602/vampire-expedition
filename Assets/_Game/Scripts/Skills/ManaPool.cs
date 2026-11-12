using System;
using Core;
using Stats;
using UnityEngine;

namespace Skills
{
    public class ManaPool : MonoBehaviour, IStatPool
    {
        [field: SerializeField] public float Max { get; private set; } = 10f;
        [field: SerializeField] public float RegenPerSecond { get; private set; } = 1f;
        [field: SerializeField] public bool StartFilled { get; private set; }
        [field: SerializeField] public float Current { get; private set; }

        public bool IsEmpty
        {
            get { return Current <= 0f; }
        }

        public bool IsFull
        {
            get { return Current >= Max; }
        }

        public event Action<float, float> Changed;
        public event Action Emptied;

        private void Awake()
        {
            if (StartFilled)
            {
                Fill();
            }
            else
            {
                Empty();
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        public void Init()
        {
            Unsubscribe();

            if (Game.Level == null || Game.Level.Clock == null)
            {
                Debug.LogWarning("[ManaPool] No CombatClock available on Game.Level.", this);
                return;
            }

            Game.Level.Clock.Ticked += OnTicked;
        }

        private void Unsubscribe()
        {
            if (Game.Level == null || Game.Level.Clock == null)
            {
                return;
            }

            Game.Level.Clock.Ticked -= OnTicked;
        }

        public void Fill()
        {
            SetCurrent(Max);
        }

        public void Empty()
        {
            SetCurrent(0f);
        }

        public void Gain(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetCurrent(Current + amount);
        }

        public void Lose(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetCurrent(Current - amount);
        }

        public bool Has(float amount)
        {
            return Current >= amount;
        }

        private void OnTicked(float dt)
        {
            if (IsFull || RegenPerSecond <= 0f)
            {
                return;
            }

            Gain(RegenPerSecond * dt);
        }

        private void SetCurrent(float value)
        {
            bool wasEmpty = IsEmpty;
            float previous = Current;

            Current = Mathf.Clamp(value, 0f, Max);

            if (!Mathf.Approximately(previous, Current))
            {
                Changed?.Invoke(Current, Max);
            }

            if (!wasEmpty && IsEmpty)
            {
                Emptied?.Invoke();
            }
        }
    }
}
