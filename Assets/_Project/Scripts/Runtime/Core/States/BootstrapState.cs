using Cysharp.Threading.Tasks;
using Scripts.Runtime.Core.Modules;
using Scripts.Runtime.UI;

namespace Scripts.Runtime.Core.States
{
    public class BootstrapState : GameState
    {
        private readonly ModulesFacade _modulesFacade;

        public BootstrapState(ModulesFacade modulesFacade)
        {
            _modulesFacade = modulesFacade;
        }

        public override void Enter()
        {
            Curtains.Instance.ShowCurtains();
            Load().Forget();
        }

        private async UniTask Load()
        {
            await _modulesFacade.Load();
            Machine.Enter<LoadMainMenuState>();
        }

        public override void Tick() { }

        public override void Exit() { }
    }
}