using Cysharp.Threading.Tasks;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.States;
using Scripts.Runtime.Utils;
using UniRx;
using Unity.Netcode;
using VContainer;

namespace Scripts.Runtime.Core
{
    public class MainMenuNetController : NetworkContext
    {
        private GameStateMachine _gameStateMachine;
        private ConnectionManager _connectionManager;

        [Inject]
        private void Construct(GameStateMachine gameStateMachine, ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _gameStateMachine = gameStateMachine;
        }

        private void Start()
        {
            _connectionManager.StartGameRequested.Subscribe(_ => StartLoadGameplayStateRpc()).AddTo(destroyCancellationToken);
        }

        // Made for sync GameStateMachine
        [Rpc(SendTo.Everyone)]
        public void StartLoadGameplayStateRpc()
        {
            _gameStateMachine.Enter<LoadGameplayState>();
        }
    }
}
