using UnityEngine;

namespace Units
{
    public class UnitAnimator : MonoBehaviour
    {
        private static readonly int IDLE_PROPERTY = Animator.StringToHash("Idle");
        private static readonly int MOVEP_ROPERTY = Animator.StringToHash("Move");
        private static readonly int ATTACK_PROPERTY = Animator.StringToHash("Attack");
        private static readonly int DIE_PROPERTY = Animator.StringToHash("Die");

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
            _controller.StateManager.GetState<UnitChaseState>().Entered += OnMoveEntered;
            _controller.StateManager.GetState<UnitChaseState>().Exited += OnMoveExited;
            _controller.Combat.AttackStarted += OnAttackStarted;
            _controller.Died += OnDied;

            _animator.SetBool(IDLE_PROPERTY, _controller.StateManager.CurrentState is UnitIdleState);
            _animator.SetBool(MOVEP_ROPERTY, _controller.StateManager.CurrentState is UnitMoveToTargetState);
        }

        private void OnDestroy()
        {
            _controller.StateManager.GetState<UnitIdleState>().Entered -= OnIdleEntered;
            _controller.StateManager.GetState<UnitIdleState>().Exited -= OnIdleExited;
            _controller.StateManager.GetState<UnitMoveToTargetState>().Entered -= OnMoveEntered;
            _controller.StateManager.GetState<UnitMoveToTargetState>().Exited -= OnMoveExited;
            _controller.StateManager.GetState<UnitChaseState>().Entered -= OnMoveEntered;
            _controller.StateManager.GetState<UnitChaseState>().Exited -= OnMoveExited;
            _controller.Combat.AttackStarted -= OnAttackStarted;
            _controller.Died -= OnDied;
        }

        private void OnDied()
        {
            _animator.ResetTrigger(ATTACK_PROPERTY);
            _animator.SetTrigger(DIE_PROPERTY);
        }

        public void AttackHit()
        {
            _controller.Combat.Hit();
        }

        private void OnAttackStarted()
        {
            _animator.SetTrigger(ATTACK_PROPERTY);
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
