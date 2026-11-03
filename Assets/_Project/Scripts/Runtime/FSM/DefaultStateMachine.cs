namespace Scripts.Runtime.FSM
{
    public class DefaultStateMachine : StateMachine<IState>
    {
        public DefaultStateMachine(IState[] states) : base(states) { }
    }
}