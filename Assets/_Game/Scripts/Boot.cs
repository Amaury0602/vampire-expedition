using Menus;
using Sequences;
using UnityEngine;

namespace Core
{
    public class Boot : MonoBehaviour
    {
        [SerializeField] private MenuDatabase _menuDatabase;
        [SerializeField] private Transform _menuRoot;
        [SerializeField] private int _gameSceneBuildIndex = 1;

        private void Start()
        {
            SequenceProvider.BootSequence(_menuDatabase, _menuRoot, _gameSceneBuildIndex).Start();
        }
    }
}
