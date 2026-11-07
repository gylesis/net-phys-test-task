using Cysharp.Threading.Tasks;
using Scripts.Runtime.Core.Modules;
using Scripts.Runtime.UI;

namespace Scripts.Runtime.Core.States
{
    public class BootstrapState : GameState
    {
        private readonly ModulesFacade _modulesFacade;
        private readonly Curtains _curtains;

        public BootstrapState(ModulesFacade modulesFacade, Curtains curtains)
        {
            _curtains = curtains;
            _modulesFacade = modulesFacade;
        }

        public override void Enter()
        {
            _curtains.ShowCurtains();
            Load().Forget();
        }

        private async UniTask Load()
        {
            await _modulesFacade.Load();
            Machine.Enter<LoadMainMenuState>();
        }

        public override void Exit() { }
    }
}
