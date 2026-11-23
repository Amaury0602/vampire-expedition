using UnityEngine;
using Worlds;

namespace Core
{
    public static class Game
    {
        public static Level Level { get; private set; }

        public static World World { get; private set; }

        public static bool IsInitialized { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Level = null;
            World = null;
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

        public static void InitWorld(WorldSettings settings, PlayerRosterSettings roster)
        {
            if (settings == null || roster == null)
            {
                Debug.LogError("[Game] InitWorld : missing WorldSettings or PlayerRosterSettings.");
                World = null;
                return;
            }

            World = new World(settings, roster);
        }
    }
}
