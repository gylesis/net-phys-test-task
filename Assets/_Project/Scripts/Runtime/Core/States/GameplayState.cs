using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.UI;
using UniRx;
using Unity.Netcode;

namespace Scripts.Runtime.Core.States
{
    public class GameplayState : GameState
    {
        private readonly ConnectionManager _connectionManager;
        private readonly UIController _uiController;
        private readonly Curtains _curtains;
        private readonly PlayerSpawner _playerSpawner;

        private CompositeDisposable _disposable;

        public GameplayState(ConnectionManager connectionManager, UIController uiController, Curtains curtains,
            PlayerSpawner playerSpawner)
        {
            _playerSpawner = playerSpawner;
            _curtains = curtains;
            _uiController = uiController;
            _connectionManager = connectionManager;
        }

        public override void Enter()
        {
            _curtains.HideCurtains();
            _uiController.ShowMenu<GameplayHUDMenu>();

            _disposable?.Dispose();
            _disposable = new CompositeDisposable();

            if (_connectionManager.IsServer)
            {
                // for late joiners
                _connectionManager.ClientConnected.Subscribe(HandleClientConnected).AddTo(_disposable);

                _connectionManager.ClientDisconnected.Subscribe(HandleClientDisconnected).AddTo(_disposable);

                foreach (var client in _connectionManager.ConnectedPlayers)
                {
                    var clientId = client.Value.ClientId;
                    HandleClientConnected(clientId);
                }
            }
        }

        private void HandleClientConnected(ulong playerId)
        {
            var playerController = _playerSpawner.SpawnPlayer(playerId);
            _connectionManager.SessionInfo.AddPlayerObject(playerController);
        }

        private void HandleClientDisconnected(ulong playerId)
        {
            _connectionManager.SessionInfo.RemovePlayerObject(playerId);
        }

        public override void Exit()
        {
            _uiController.HideAllActiveMenus();

            if (NetworkManager.Singleton.IsServer)
            {
                _disposable?.Dispose();
                _disposable = null;
            }
        }
    }
}
