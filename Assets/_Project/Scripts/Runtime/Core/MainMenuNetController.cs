using Scripts.Runtime.Core.States;
using Scripts.Runtime.Utils;
using Unity.Netcode;
using VContainer;

namespace Scripts.Runtime.Core
{
    public class MainMenuNetController : NetworkContext
    {
        private GameStateMachine _gameStateMachine;

        [Inject]
        private void Construct(GameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        // Made for sync GameStateMachine
        [Rpc(SendTo.Everyone)]
        public void StartLoadGameplayStateRpc()
        {
            _gameStateMachine.Enter<LoadGameplayState>();
        }
        
    }
}