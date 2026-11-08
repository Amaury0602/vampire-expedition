using System;
using Damages;
using Stats;
using UnityEngine;

namespace Units
{
    public class Health : MonoBehaviour, IStatPool
    {
        [field: SerializeField] public float Max { get; private set; } = 10f;
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
        public event Action<DamageInfo> Damaged;

        private void Awake()
        {
            Fill();
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
            if (amount <= 0f || IsEmpty)
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

        public void TakeDamage(DamageInfo damage)
        {
            if (IsEmpty || damage.Amount <= 0f)
            {
                return;
            }

            Lose(damage.Amount);
            Damaged?.Invoke(damage);
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
