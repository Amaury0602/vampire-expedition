using System;
using Units;
using UnityEngine;

namespace Characters
{
    [RequireComponent(typeof(UnitController))]
    public class Hero : MonoBehaviour
    {
        [field: SerializeField] public CharacterSettings Settings { get; private set; }

        public UnitController Controller { get; private set; }

        public string Name
        {
            get { return Settings != null ? Settings.Name : name; }
        }

        public event Action<Hero> Died;

        private void Awake()
        {
            Controller = GetComponent<UnitController>();
            Controller.Died += OnControllerDied;
        }

        private void OnDestroy()
        {
            if (Controller != null)
            {
                Controller.Died -= OnControllerDied;
            }
        }

        private void OnControllerDied()
        {
            Died?.Invoke(this);
        }
    }
}
