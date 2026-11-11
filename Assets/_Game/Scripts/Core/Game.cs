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

        public static void Init()
        {
            Level = Object.FindAnyObjectByType<Level>();

            if (Level == null)
            {
                Debug.LogError("[Game] Init : no Level found in loaded scenes.");
                IsInitialized = false;
                return;
            }

            Level.Init();
            IsInitialized = true;
        }
    }
}
