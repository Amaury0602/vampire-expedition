using UnityEngine;

namespace Units
{
    public class UnitAnimator : MonoBehaviour
    {
        private static readonly int IDLE_PROPERTY = Animator.StringToHash("Idle");
        private static readonly int MOVEP_ROPERTY = Animator.StringToHash("Move");

        [SerializeField] private UnitController _controller;
        [SerializeField] private Animator _animator;

        private void Reset()
        {
            _controller = GetComponentInParent<UnitController>();
            _animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            _controller.StateManager.GetState<UnitIdleState>().Entered += OnIdleEntered;
            _controller.StateManager.GetState<UnitIdleState>().Exited += OnIdleExited;
            _controller.StateManager.GetState<UnitMoveToTargetState>().Entered += OnMoveEntered;
            _controller.StateManager.GetState<UnitMoveToTargetState>().Exited += OnMoveExited;

            _animator.SetBool(IDLE_PROPERTY, _controller.StateManager.CurrentState is UnitIdleState);
            _animator.SetBool(MOVEP_ROPERTY, _controller.StateManager.CurrentState is UnitMoveToTargetState);
        }

        private void OnDestroy()
        {
            _controller.StateManager.GetState<UnitIdleState>().Entered -= OnIdleEntered;
            _controller.StateManager.GetState<UnitIdleState>().Exited -= OnIdleExited;
            _controller.StateManager.GetState<UnitMoveToTargetState>().Entered -= OnMoveEntered;
            _controller.StateManager.GetState<UnitMoveToTargetState>().Exited -= OnMoveExited;
        }

        private void OnIdleEntered()
        {
            _animator.SetBool(IDLE_PROPERTY, true);
        }

        private void OnIdleExited()
        {
            _animator.SetBool(IDLE_PROPERTY, false);
        }

        private void OnMoveEntered()
        {
            _animator.SetBool(MOVEP_ROPERTY, true);
        }

        private void OnMoveExited()
        {
            _animator.SetBool(MOVEP_ROPERTY, false);
        }
    }
}
