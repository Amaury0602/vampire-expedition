using Menus;
using Sequences;
using UnityEngine;
using Worlds;

namespace Core
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private MenuDatabase _menuDatabase;
        [SerializeField] private Transform _menuRoot;
        [SerializeField] private int _gameSceneBuildIndex = 1;
        [SerializeField] private WorldSettings _world;
        [SerializeField] private PlayerRosterSettings _roster;

        private void Start()
        {
            SequenceProvider.BootSequence(_menuDatabase, _menuRoot, _gameSceneBuildIndex, _world, _roster).Start();
        }
    }
}
