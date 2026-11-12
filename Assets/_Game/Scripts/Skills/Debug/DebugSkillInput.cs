using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Skills
{
    public class DebugSkillInput : MonoBehaviour
    {
        [SerializeField] private SkillCaster _caster;

        private void Reset()
        {
            _caster = GetComponent<SkillCaster>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || _caster == null)
            {
                return;
            }

            bool levelUp = keyboard.shiftKey.isPressed;

            TryUse(keyboard.f1Key, 0, levelUp);
            TryUse(keyboard.f2Key, 1, levelUp);
            TryUse(keyboard.f3Key, 2, levelUp);
            TryUse(keyboard.f4Key, 3, levelUp);
        }

        private void TryUse(KeyControl key, int index, bool levelUp)
        {
            if (!key.wasPressedThisFrame)
            {
                return;
            }

            IReadOnlyList<SkillSlot> slots = _caster.Slots;
            if (index >= slots.Count)
            {
                Debug.Log($"[DebugSkillInput] No skill slot {index}.");
                return;
            }

            SkillSlot slot = slots[index];
            if (levelUp)
            {
                int next = slot.Level >= slot.Settings.MaxLevel ? 1 : slot.Level + 1;
                slot.SetLevel(next);
                Debug.Log($"[DebugSkillInput] '{slot.Settings.Name}' level {slot.Level}/{slot.Settings.MaxLevel}.");
                return;
            }

            if (!_caster.BeginCast(slot))
            {
                Debug.Log($"[DebugSkillInput] Cannot cast '{slot.Settings.Name}' ({slot.Owner.Name}).");
            }
        }
    }
}
