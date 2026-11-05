using System.Collections.Generic;
using UnityEngine;

namespace Menus
{
    [CreateAssetMenu(fileName = "MenuDatabase", menuName = "SO/Menus/MenuDatabase")]
    public class MenuDatabase : ScriptableObject
    {
        [SerializeField] private List<Menu> _menuPrefabs = new();

        public IReadOnlyList<Menu> MenuPrefabs => _menuPrefabs;
    }
}
