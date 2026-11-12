using System;
using System.Collections.Generic;
using Battle;
using Characters;
using Core;
using Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skills
{
    public class SkillCaster : MonoBehaviour
    {
        [SerializeField] private SkillTargetingPreview _preview;
        [SerializeField] private Camera _camera;

        public IReadOnlyList<SkillSlot> Slots
        {
            get { return _slots; }
        }

        public bool IsTargeting
        {
            get { return _currentSlot != null; }
        }

        public SkillSlot CurrentSlot
        {
            get { return _currentSlot; }
        }

        public event Action SlotsChanged;
        public event Action<SkillSlot> CastStarted;
        public event Action<SkillSlot, bool> CastEnded;

        private readonly List<SkillSlot> _slots = new List<SkillSlot>();
        private readonly SkillContext _context = new SkillContext();
        private SkillSlot _currentSlot;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        public void Init()
        {
            Unsubscribe();

            Battlefield battlefield = Game.Level.Battlefield;
            if (battlefield == null)
            {
                Debug.LogWarning("[SkillCaster] No Battlefield available on Game.Level.", this);
                return;
            }

            battlefield.UnitJoined += OnUnitJoined;
            battlefield.UnitRemoved += OnUnitRemoved;

            IReadOnlyList<UnitController> units = battlefield.PlayerUnits;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] != null && units[i].Combat.Battlefield == battlefield)
                {
                    OnUnitJoined(units[i]);
                }
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
            CancelCast();
        }

        private void Unsubscribe()
        {
            if (Game.Level == null || Game.Level.Battlefield == null)
            {
                return;
            }

            Game.Level.Battlefield.UnitJoined -= OnUnitJoined;
            Game.Level.Battlefield.UnitRemoved -= OnUnitRemoved;
        }

        private void Update()
        {
            if (!IsTargeting)
            {
                return;
            }

            if (!CanCast(_currentSlot))
            {
                CancelCast();
                return;
            }

            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;

            bool cancelPressed = (mouse != null && mouse.rightButton.wasPressedThisFrame) || (keyboard != null && keyboard.escapeKey.wasPressedThisFrame);
            if (cancelPressed)
            {
                CancelCast();
                return;
            }

            if (mouse == null || _camera == null)
            {
                return;
            }

            Vector2 position = _camera.ScreenToWorldPoint(mouse.position.ReadValue());
            bool valid = UpdateTargeting(position, out UnitController targetUnit);

            if (valid && mouse.leftButton.wasPressedThisFrame)
            {
                Execute(_currentSlot, targetUnit, position);
                EndCast(true);
            }
        }

        public bool CanCast(SkillSlot slot)
        {
            if (slot == null || slot.Settings == null || slot.Owner == null || Game.Level == null || Game.Level.Mana == null || Game.Level.Battlefield == null)
            {
                return false;
            }

            SkillLevel data = slot.Data;
            if (data == null)
            {
                return false;
            }

            return Targeting.IsValidTarget(slot.Owner.Controller) && Game.Level.Mana.Has(data.ManaCost);
        }

        public bool BeginCast(SkillSlot slot)
        {
            if (IsTargeting)
            {
                CancelCast();
            }

            if (!CanCast(slot))
            {
                return false;
            }

            if (slot.Settings.Targeting == SkillTargeting.Instant)
            {
                CastStarted?.Invoke(slot);
                Execute(slot, null, slot.Owner.Controller.Hitbox.Center);
                CastEnded?.Invoke(slot, true);
                return true;
            }

            _currentSlot = slot;
            CastStarted?.Invoke(slot);
            return true;
        }

        public void CancelCast()
        {
            if (!IsTargeting)
            {
                return;
            }

            EndCast(false);
        }

        private void EndCast(bool success)
        {
            SkillSlot slot = _currentSlot;
            _currentSlot = null;

            if (_preview != null)
            {
                _preview.Hide();
            }

            CastEnded?.Invoke(slot, success);
        }

        private bool UpdateTargeting(Vector2 position, out UnitController targetUnit)
        {
            targetUnit = null;
            SkillSettings settings = _currentSlot.Settings;
            UnitController caster = _currentSlot.Owner.Controller;

            if (settings.Targeting == SkillTargeting.Area)
            {
                _preview.ShowArea(position, _currentSlot.Data.Radius, true);
                return true;
            }

            targetUnit = FindUnitAt(position, caster, settings.TargetFilter);

            if (targetUnit != null)
                _preview.ShowUnit(targetUnit, true);
            else
                _preview.ShowCursor(position);

            return targetUnit != null;
        }

        private void Execute(SkillSlot slot, UnitController targetUnit, Vector2 position)
        {
            SkillLevel data = slot.Data;

            _context.Clear();
            _context.Settings = slot.Settings;
            _context.LevelData = data;
            _context.Level = slot.Level;
            _context.Caster = slot.Owner.Controller;
            _context.TargetUnit = targetUnit;
            _context.Position = position;

            CollectTargets(_context);

            Game.Level.Mana.Lose(data.ManaCost);

            for (int i = 0; i < data.Effects.Count; i++)
            {
                SkillEffect effect = data.Effects[i];
                if (effect == null)
                {
                    continue;
                }

                effect.OnCast(_context);

                for (int j = 0; j < _context.Targets.Count; j++)
                {
                    UnitController target = _context.Targets[j];
                    if (Targeting.IsValidTarget(target))
                    {
                        effect.Apply(_context, target);
                    }
                }
            }

            _context.Clear();
        }

        private void CollectTargets(SkillContext context)
        {
            SkillSettings settings = context.Settings;

            if (settings.TargetFilter == SkillTargetFilter.Self)
            {
                context.Targets.Add(context.Caster);
                return;
            }

            if (settings.Targeting == SkillTargeting.Unit)
            {
                if (context.TargetUnit != null)
                {
                    context.Targets.Add(context.TargetUnit);
                }

                return;
            }

            CollectFrom(Game.Level.Battlefield.PlayerUnits, context);
            CollectFrom(Game.Level.Battlefield.OtherUnits, context);
        }

        private void CollectFrom(IReadOnlyList<UnitController> units, SkillContext context)
        {
            SkillSettings settings = context.Settings;

            for (int i = 0; i < units.Count; i++)
            {
                UnitController unit = units[i];
                if (!Targeting.IsValidTarget(unit) || !MatchesFilter(context.Caster, unit, settings.TargetFilter))
                {
                    continue;
                }

                if (settings.Targeting == SkillTargeting.Area && !IsInArea(unit, context.Position, context.Radius))
                {
                    continue;
                }

                context.Targets.Add(unit);
            }
        }

        private UnitController FindUnitAt(Vector2 position, UnitController caster, SkillTargetFilter filter)
        {
            UnitController closest = null;
            float closestSqrDistance = float.MaxValue;

            FindClosestIn(Game.Level.Battlefield.PlayerUnits, position, caster, filter, ref closest, ref closestSqrDistance);
            FindClosestIn(Game.Level.Battlefield.OtherUnits, position, caster, filter, ref closest, ref closestSqrDistance);

            return closest;
        }

        private void FindClosestIn(IReadOnlyList<UnitController> units, Vector2 position, UnitController caster, SkillTargetFilter filter, ref UnitController closest, ref float closestSqrDistance)
        {
            for (int i = 0; i < units.Count; i++)
            {
                UnitController unit = units[i];
                if (!Targeting.IsValidTarget(unit) || !MatchesFilter(caster, unit, filter))
                {
                    continue;
                }

                float sqrDistance = (unit.Hitbox.Center - position).sqrMagnitude;
                float radius = unit.Hitbox.Radius;
                if (sqrDistance <= radius * radius && sqrDistance < closestSqrDistance)
                {
                    closest = unit;
                    closestSqrDistance = sqrDistance;
                }
            }
        }

        private static bool MatchesFilter(UnitController caster, UnitController unit, SkillTargetFilter filter)
        {
            switch (filter)
            {
                case SkillTargetFilter.Self:
                    return unit == caster;
                case SkillTargetFilter.Allies:
                    return unit.Combat.Team == caster.Combat.Team;
                case SkillTargetFilter.Enemies:
                    return unit.Combat.Team != caster.Combat.Team;
                default:
                    return true;
            }
        }

        private static bool IsInArea(UnitController unit, Vector2 center, float radius)
        {
            float reach = radius + unit.Hitbox.Radius;
            return (unit.Hitbox.Center - center).sqrMagnitude <= reach * reach;
        }

        private void OnUnitJoined(UnitController unit)
        {
            if (unit.Combat.Team != Team.Player || !unit.TryGetComponent(out Hero hero) || hero.Settings == null)
                return;

            if (HasSlots(hero))
                return;

            List<SkillSettings> skills = hero.Settings.Skills;
            for (int i = 0; i < skills.Count; i++)
            {
                if (skills[i] != null)
                {
                    _slots.Add(new SkillSlot(hero, skills[i]));
                }
            }

            SlotsChanged?.Invoke();
        }

        private void OnUnitRemoved(UnitController unit)
        {
            bool removed = false;

            for (int i = _slots.Count - 1; i >= 0; i--)
            {
                if (_slots[i].Owner.Controller != unit)
                {
                    continue;
                }

                if (_slots[i] == _currentSlot)
                {
                    CancelCast();
                }

                _slots.RemoveAt(i);
                removed = true;
            }

            if (removed)
            {
                SlotsChanged?.Invoke();
            }
        }

        private bool HasSlots(Hero hero)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].Owner == hero)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
