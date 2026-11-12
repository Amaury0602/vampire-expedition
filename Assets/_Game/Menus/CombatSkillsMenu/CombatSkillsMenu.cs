using Pooling;
using System.Collections.Generic;
using UnityEngine;
using Core;

namespace Menus
{
    public class CombatSkillsMenu : Menu
    {
        [SerializeField] private SkillCard _skillCardPrefab;
        private List<SkillCard> _cards = new List<SkillCard>();
        [SerializeField] private RectTransform _cardsContainer;


        private void Awake()
        {
            Pooler.SetPool(_skillCardPrefab, 8);
        }


        public override void Show()
        {
            base.Show();
            ClearCards();
            Game.Level.SkillCaster.SlotsChanged += UpdateSlots;
            UpdateSlots();
        }

        public override void Hide()
        {
            base.Hide();
            Game.Level.SkillCaster.SlotsChanged -= UpdateSlots;
        }

        private void ClearCards()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].gameObject.Release();
            }

            _cards.Clear();
        }

        private void UpdateSlots()
        {
            var slots = Game.Level.SkillCaster.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                var card = _skillCardPrefab.Get();
                card.transform.SetParent(_cardsContainer);
                card.Setup(slots[i]);
                _cards.Add(card);
            }
        }
    }
}
