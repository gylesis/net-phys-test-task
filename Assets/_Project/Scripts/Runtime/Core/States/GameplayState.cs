using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.UI;
using Scripts.Runtime.Utils;
using Unity.Netcode;

namespace Scripts.Runtime.Core.States
{
    public class GameplayState : GameState
    {
        private PlayerSpawner _playerSpawner;

        public override void Enter()
        {
            Curtains.Instance.HideCurtains();
            UIController.Instance.ShowMenu<GameplayHUDMenu>();

            _playerSpawner = DiInjecter.Instance.Resolve<PlayerSpawner>();

            if (NetworkManager.Singleton.IsServer)
            {
                // for late joiners
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
                
                foreach (var client in NetworkManager.Singleton.ConnectedClients)
                {
                    var clientId = client.Value.ClientId;
                    HandleClientConnected(clientId);
                }
            }
        }

        private void HandleClientConnected(ulong playerId)
        {
            _playerSpawner.SpawnPlayer(playerId);
        }

        public override void Tick() { }

        public override void Exit()
        {
            UIController.Instance.HideAllActiveMenus();
            
            if (NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            }
        }
    }
}
