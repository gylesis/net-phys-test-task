using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Bootstrap;
using Scripts.Runtime.UI;
using UnityEngine;

namespace Scripts.Runtime.Core.States
{
    public class BootstrapState : GameState
    {
        private IReadOnlyList<IBootstrapModule> _bootstrapModules;

        public BootstrapState(IReadOnlyList<IBootstrapModule> bootstrapModules)
        {
            _bootstrapModules = bootstrapModules;
        }

        public override void Enter()
        {
            Curtains.Instance.ShowCurtains();
            Load().Forget();
        }

        private async UniTask Load()
        {
            foreach (var bootstrapModule in _bootstrapModules)
            {
                await bootstrapModule.InitializeAsync(Application.exitCancellationToken);
            }

            Machine.Enter<LoadMainMenuState>();
        }

        public override void Tick() { }

        public override void Exit() { }
    }
}