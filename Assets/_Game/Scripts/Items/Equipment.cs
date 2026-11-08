using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public class Equipment : MonoBehaviour
    {
        [field: SerializeField] public List<Item> Items { get; private set; } = new List<Item>();

        public Weapon Weapon { get; private set; }

        private void Awake()
        {
            RefreshWeapon();
        }

        private void OnValidate()
        {
            RefreshWeapon();
        }

        public void AddItem(Item item)
        {
            if (item == null)
            {
                return;
            }

            Items.Add(item);
            RefreshWeapon();
        }

        public void RefreshWeapon()
        {
            Weapon = null;

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] is Weapon weapon)
                {
                    Weapon = weapon;
                    return;
                }
            }
        }
    }
}
