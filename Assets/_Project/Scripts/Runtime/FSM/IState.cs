namespace Scripts.Runtime.FSM
{
    public interface IState
    {
        void Enter();
        void Tick();
        void Exit();
    }

    public interface IDrawGizmos
    {
        void DrawGizmos();
    }
}