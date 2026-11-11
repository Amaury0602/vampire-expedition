using Menus;
using UnityEngine;

namespace Sequences
{
    public class InitMenusHandler : Handler
    {
        private readonly MenuDatabase _database;
        private readonly Transform _root;

        public InitMenusHandler(MenuDatabase database, Transform root)
        {
            _database = database;
            _root = root;
        }

        protected override void OnEnter()
        {
            MenuManager.Init(_database, _root);
            Continue();
        }
    }
}
