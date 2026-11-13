using Pooling;
using System.Collections.Generic;
using UnityEngine;
using Core;
using System;

namespace Menus
{
    public class CombatSkillsMenu : Menu
    {
        [SerializeField] private SkillCard _skillCardPrefab;
        private List<SkillCard> _cards = new List<SkillCard>();
        [SerializeField] private RectTransform _cardsContainer;

        [SerializeField] private ManaPoolUICounter _manaCounter;


        private void Awake()
        {
            Pooler.SetPool(_skillCardPrefab, 8);
        }


        public override void Show()
        {
            base.Show();

            _manaCounter.Init();

            ClearCards();
            Game.Level.SkillCaster.SlotsChanged += UpdateSlots;
            Game.Level.Mana.Changed += OnManaChanged;
            UpdateSlots();
        }

        public override void Hide()
        {
            base.Hide();
            _manaCounter.UnSubscribe();
            Game.Level.SkillCaster.SlotsChanged -= UpdateSlots;
            Game.Level.Mana.Changed -= OnManaChanged;
        }

        private void OnCardClicked(SkillCard card)
        {
            Game.Level.SkillCaster.BeginCast(card.Skill);
        }

        private void OnManaChanged(float current, float max)
        {
            RefreshLocks();
        }

        private void RefreshLocks()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                SkillCard card = _cards[i];
                card.SetLocked(!Game.Level.Mana.Has(card.Skill.Data.ManaCost));
            }
        }

        private void ClearCards()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].Clicked -= OnCardClicked;
                _cards[i].gameObject.Release();
            }

            _cards.Clear();
        }

        private void UpdateSlots()
        {
            ClearCards();

            var slots = Game.Level.SkillCaster.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                var card = _skillCardPrefab.Get();
                card.transform.SetParent(_cardsContainer);
                card.Setup(slots[i]);
                card.Clicked += OnCardClicked;
                _cards.Add(card);
            }

            RefreshLocks();
        }
    }
}
