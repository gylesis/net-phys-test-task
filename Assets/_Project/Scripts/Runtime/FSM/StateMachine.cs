using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Scripts.Runtime.FSM
{
    public class StateMachine<TState> where TState : class, IState
    {
        private Dictionary<Type, TState> _states;

        private TState _currentState;
        private TState _prevState;
        
        public string CurrentStateName { get; private set; }
        public Subject<TState> OnStateEntered { get; } = new();

        public StateMachine(IReadOnlyList<TState> states)
        {
            _states = states.ToDictionary(x => x.GetType());
        }

        /// <summary>Экземпляр состояния — например, чтобы передать ему параметры перед входом.</summary>
        public TStateType Get<TStateType>() where TStateType : TState
        {
            return (TStateType)_states[typeof(TStateType)];
        }

        public void Enter<TStateType>() where TStateType : TState
        {
            var state = _states[typeof(TStateType)];
            EnterState(state);
        }
        
        /// <summary>
        /// Войти в стейт по типу — для синхронизации по сети (NpcStateMachineSyncer), где тип
        /// известен только в рантайме. Неизвестный тип молча пропускаем.
        /// </summary>
        public void Enter(Type stateType)
        {
            if (stateType == null || !_states.TryGetValue(stateType, out var state))
                return;

            EnterState(state);
        }

        public void EnterPrevious()
        {
            if (_prevState != null)
                EnterState(_prevState);
        }

        public bool IsPreviosStateIs<TStateType>() where TStateType : TState
        {
            var state = _states[typeof(TStateType)];
            return _prevState == state;
        }

        public void Exit()
        {
            _currentState?.Exit();
            _currentState = null;
            CurrentStateName = null;
            OnStateEntered.OnNext(null);
        }

        private void EnterState(TState state)
        {
            if (_currentState == state)
                return;

            _currentState?.Exit();
            _prevState = _currentState;
            
            _currentState = state;
            _currentState.Enter();
            CurrentStateName = _currentState.GetType().Name; // TODO only editor
            OnStateEntered.OnNext(_currentState);
        }
    }
}