using Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle
{
    public class CombatTimeControls : MonoBehaviour
    {
        [SerializeField] private float[] _speeds = { 1f, 2f, 4f };

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || Game.Level == null || Game.Level.Clock == null)
            {
                return;
            }

            CombatClock clock = Game.Level.Clock;

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                clock.TogglePause();
            }

            TrySetSpeed(clock, keyboard.digit1Key, 0);
            TrySetSpeed(clock, keyboard.digit2Key, 1);
            TrySetSpeed(clock, keyboard.digit3Key, 2);
        }

        private void TrySetSpeed(CombatClock clock, UnityEngine.InputSystem.Controls.KeyControl key, int index)
        {
            if (!key.wasPressedThisFrame || index >= _speeds.Length)
            {
                return;
            }

            clock.SetSpeed(_speeds[index]);
        }
    }
}
