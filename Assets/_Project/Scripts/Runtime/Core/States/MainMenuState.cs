using Scripts.Runtime.UI;

namespace Scripts.Runtime.Core.States
{
    public class MainMenuState : GameState
    {
        private readonly UIController _uiController;
        private readonly Curtains _curtains;

        public MainMenuState(UIController uiController, Curtains curtains)
        {
            _curtains = curtains;
            _uiController = uiController;
        }

        public override void Enter()
        {
            _curtains.HideCurtains();
            _uiController.ShowMenu<MainMenu>();
        }

        public override void Exit() { }
    }
}
