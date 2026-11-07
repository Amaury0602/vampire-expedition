using UnityEngine;
using UnityEngine.InputSystem;

namespace Units
{
    public class DebugTargetSetter : MonoBehaviour
    {
        [SerializeField] private UnitController[] _units;
        [SerializeField] private Camera _camera;

        private void Awake()
        {
            if (_camera == null)
                _camera = Camera.main;
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector2 worldPosition = _camera.ScreenToWorldPoint(screenPosition);


            for (int i = 0; i < _units.Length; i++)
            {
                _units[i].MoveToTarget(worldPosition);
            }


            Debug.Log(worldPosition);
        }
    }
}
