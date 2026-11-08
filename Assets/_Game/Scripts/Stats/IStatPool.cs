using System;

namespace Stats
{
    public interface IStatPool
    {
        float Current { get; }
        float Max { get; }
        bool IsEmpty { get; }
        bool IsFull { get; }

        event Action<float, float> Changed;
        event Action Emptied;

        void Fill();
        void Empty();
        void Gain(float amount);
        void Lose(float amount);
    }
}
