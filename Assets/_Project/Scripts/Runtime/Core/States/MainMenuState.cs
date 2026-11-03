using Scripts.Runtime.UI;

namespace Scripts.Runtime.Core.States
{
    public class MainMenuState : GameState
    {
        public override void Enter()
        {
            UIController.Instance.ShowMenu<MainMenu>();
        }

        public override void Tick() { }

        public override void Exit() { }
    }
}