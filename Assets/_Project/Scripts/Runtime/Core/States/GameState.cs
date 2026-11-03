using Scripts.Runtime.FSM;

namespace Scripts.Runtime.Core.States
{
    public abstract class GameState : State
    {
        public GameStateMachine Machine { get; set; }
    }
}
