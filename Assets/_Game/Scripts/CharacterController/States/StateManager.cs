using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class StateManager
    {
        public bool IsLocked { get; private set; }
        public BaseState CurrentState { get; private set; }
        public BaseState PreviousState { get; private set; }

        [SerializeField, ReadOnly] private string _currentStateName;

        private Dictionary<Type, BaseState> _states = new Dictionary<Type, BaseState>();

        public event Action<bool> LockedEvent;

        public bool Initialized = false;
        public void Initialize<S>(StateParameters parameters = null) where S : BaseState
        {
            if (Initialized)
                return;

            Initialized = true;

            if (_states.TryGetValue(typeof(S), out BaseState state))
            {
                CurrentState = state;
                _currentStateName = CurrentState.GetType().Name;
                PreviousState = state;
            }
            CurrentState.EnterState(parameters);
        }

        public void AddState<S>(S state) where S : BaseState
        {
            if (GetState<S>() != null)
                return;

            _states.Add(state.GetType(), state);
        }

        public S GetState<S>() where S : BaseState
        {
            if (_states.TryGetValue(typeof(S), out BaseState state))
            {
                return state as S;
            }
            return null;
        }

        public BaseState GetState(BaseState state)
        {
            if (_states.TryGetValue(state.GetType(), out BaseState s))
                return state;

            return null;
        }

        public bool SetState<S>(StateParameters parameters = null) where S : BaseState
        {
            if (CurrentState != null && CurrentState.GetType() == typeof(S))
                return false;

            if (_states.TryGetValue(typeof(S), out BaseState state))
            {
                if (CurrentState != null)
                {
                    PreviousState = CurrentState;
                    CurrentState = state;
                    PreviousState.ExitState();
                }

                CurrentState = state;
                _currentStateName = CurrentState.GetType().Name;
                CurrentState.EnterState(parameters);

                return true;
            }

            return false;
        }

        public bool SetState(BaseState state, StateParameters parameters = null)
        {
            if (CurrentState != null && CurrentState == state)
                return false;

            if (_states.TryGetValue(state.GetType(), out BaseState outState))
            {
                if (CurrentState != null)
                {
                    PreviousState = CurrentState;
                    CurrentState = outState;
                    PreviousState.ExitState();
                }

                CurrentState = outState;
                _currentStateName = CurrentState.GetType().Name;
                CurrentState.EnterState(parameters);

                return true;
            }

            return false;
        }

        public void Lock(bool value)
        {
            IsLocked = value;
            LockedEvent?.Invoke(value);
        }

        public void Update(float dt)
        {
            if (CurrentState != null && !IsLocked)
                CurrentState.UpdateState(dt);
        }
        
        public void ClearEvents()
        {
            if (_states == null)
                return;

            foreach (var state in _states.Values)
                state.ClearEvents();
        }
    }

    public class StateParameters {}
}