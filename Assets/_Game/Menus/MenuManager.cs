using System;
using System.Collections.Generic;
using UnityEngine;

namespace Menus
{
    /// <summary>
    /// Point d'accès local (per-client, non-réseau) aux menus UI.
    /// Initialisé depuis un MenuDatabase : chaque prefab Menu est instancié une fois,
    /// puis retrouvable et pilotable par son type concret.
    /// </summary>
    public static class MenuManager
    {
        private static readonly Dictionary<Type, Menu> _menus = new();

        public static bool IsInitialized { get; private set; }

        // Survit au mode "Enter Play Mode sans domain reload" : sans ça l'état statique
        // persisterait entre deux sessions Play dans l'éditeur.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _menus.Clear();
            IsInitialized = false;
        }

        public static void Init(MenuDatabase database, Transform parent)
        {
            if (IsInitialized)
                return;

            if (database == null)
            {
                Debug.LogError("[MenuManager] Init called with a null MenuDatabase.");
                return;
            }

            _menus.Clear();

            foreach (Menu prefab in database.MenuPrefabs)
            {
                if (prefab == null)
                    continue;

                Menu instance = UnityEngine.Object.Instantiate(prefab, parent);
                Type type = instance.GetType();

                if (_menus.ContainsKey(type))
                {
                    Debug.LogWarning($"[MenuManager] Duplicate menu type '{type.Name}' in database — ignoring the extra one.");
                    UnityEngine.Object.Destroy(instance.gameObject);
                    continue;
                }

                instance.gameObject.SetActive(false);

                _menus.Add(type, instance);
            }

            IsInitialized = true;
        }

        public static T Get<T>() where T : Menu
        {
            if (_menus.TryGetValue(typeof(T), out Menu menu))
                return (T)menu;

            Debug.LogWarning($"[MenuManager] No menu of type '{typeof(T).Name}' registered.");
            return null;
        }

        public static void Show<T>() where T : Menu
        {
            Get<T>()?.Show();
        }

        public static void Hide<T>() where T : Menu
        {
            Get<T>()?.Hide();
        }
    }
}
