using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.UI;
using Scripts.Runtime.Utils;
using UniRx;
using Unity.Netcode;

namespace Scripts.Runtime.Core.States
{
    public class GameplayState : GameState
    {
        private PlayerSpawner _playerSpawner;
        private CompositeDisposable _disposable;

        public override void Enter()
        {
            Curtains.Instance.HideCurtains();
            UIController.Instance.ShowMenu<GameplayHUDMenu>();

            _disposable ??= new CompositeDisposable();
            _playerSpawner ??= DiInjecter.Instance.Resolve<PlayerSpawner>();

            if (ConnectionManager.Instance.IsServer)
            {
                // for late joiners
                ConnectionManager.Instance.ClientConnected.Subscribe(HandleClientConnected).AddTo(_disposable);
                
                ConnectionManager.Instance.ClientDisconnected.Subscribe(HandleClientDisconnected).AddTo(_disposable);

                foreach (var client in ConnectionManager.Instance.ConnectedPlayers)
                {
                    var clientId = client.Value.ClientId;
                    HandleClientConnected(clientId);
                }
            }
        }

        private void HandleClientConnected(ulong playerId)
        {
            var playerController = _playerSpawner.SpawnPlayer(playerId);
            SessionInfo.Instance.AddPlayerObject(playerController);
        }

        private void HandleClientDisconnected(ulong playerId)
        {
            SessionInfo.Instance.RemovePlayerObject(playerId);
        }

        public override void Tick() { }

        public override void Exit()
        {
            UIController.Instance.HideAllActiveMenus();

            if (NetworkManager.Singleton.IsServer)
            {
                _disposable?.Dispose();
            }
        }
    }
}
