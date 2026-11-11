using UnityEngine;

namespace Core
{
    public static class Game
    {
        public static Level Level { get; private set; }

        public static bool IsInitialized { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Level = null;
            IsInitialized = false;
        }

        public static void Init(Level level)
        {
            if (level == null)
            {
                Debug.LogError("[Game] Init called with a null Level.");
                return;
            }

            Level = level;
            IsInitialized = true;
        }
    }
}
