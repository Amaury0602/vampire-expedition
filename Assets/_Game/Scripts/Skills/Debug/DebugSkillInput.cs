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

            TryCast(keyboard.f1Key, 0);
            TryCast(keyboard.f2Key, 1);
            TryCast(keyboard.f3Key, 2);
            TryCast(keyboard.f4Key, 3);
        }

        private void TryCast(KeyControl key, int index)
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
            if (!_caster.BeginCast(slot))
            {
                Debug.Log($"[DebugSkillInput] Cannot cast '{slot.Settings.Name}' ({slot.Owner.Name}).");
            }
        }
    }
}
