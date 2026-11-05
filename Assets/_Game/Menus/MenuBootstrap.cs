using UnityEngine;

namespace Menus
{
    /// <summary>
    /// Déclenche l'initialisation du MenuManager statique depuis la scène
    /// (une classe statique ne peut pas s'auto-initialiser).
    /// </summary>
    public class MenuBootstrap : MonoBehaviour
    {
        [SerializeField] private MenuDatabase _database;
        [SerializeField] private Transform _menuRoot; // empty GameObject conteneur

        private void Awake() 
        {
            MenuManager.Init(_database, _menuRoot);
            DontDestroyOnLoad(gameObject);
        }


        private void Start()
        {
        }
    }
}
