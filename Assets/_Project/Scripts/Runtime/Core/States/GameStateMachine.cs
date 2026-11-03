using System.Collections.Generic;
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
    }
}
