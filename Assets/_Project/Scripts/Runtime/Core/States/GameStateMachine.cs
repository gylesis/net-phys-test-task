using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Runtime.FSM;

namespace Scripts.Runtime.Core.States
{
    public class GameStateMachine : StateMachine<GameState>
    {
        public GameStateMachine(IReadOnlyList<GameState> states) : base(states) { }

        public new void Enter<TStateType>() where TStateType : GameState
        {
            Get<TStateType>().Machine = this;
            base.Enter<TStateType>();
        }

        public new void Enter(Type stateType)
        {
            var state = AllStates.FirstOrDefault(s => s.GetType() == stateType);
            if (state != null)
                state.Machine = this;

            base.Enter(stateType);
        }
    }
}
