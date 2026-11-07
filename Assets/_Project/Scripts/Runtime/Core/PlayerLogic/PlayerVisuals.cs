using System.Linq;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Utils;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.Core.PlayerLogic
{
    public class PlayerVisuals : NetworkContext
    {
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer _bodyRenderer;

        private GameConfig _gameConfig;
        private ConnectionManager _connectionManager;
        private SessionInfo _sessionInfo;

        [Inject]
        private void Construct(GameConfig gameConfig, ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _gameConfig = gameConfig;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            EnsureInjected();

            _sessionInfo = _connectionManager != null ? _connectionManager.SessionInfo : null;
            if (_sessionInfo != null)
                _sessionInfo.PlayersInfo.OnListChanged += HandlePlayersInfoChanged;

            UpdateSkin();
        }

        public override void OnNetworkDespawn()
        {
            if (_sessionInfo != null)
                _sessionInfo.PlayersInfo.OnListChanged -= HandlePlayersInfoChanged;

            _sessionInfo = null;
            base.OnNetworkDespawn();
        }

        private void HandlePlayersInfoChanged(NetworkListEvent<PlayerInfo> changeEvent)
        {
            UpdateSkin();
        }

        private void UpdateSkin()
        {
            if (_sessionInfo == null)
                return;

            foreach (var playerInfo in _sessionInfo.PlayersInfo)
            {
                if (playerInfo.ClientId != NetworkObject.OwnerClientId)
                    continue;

                UpdateBodyColor((SkinType)playerInfo.SkinIndex);
                return;
            }
        }
        
        public void UpdateBodyColor(SkinType skinType)
        {
            var playerVisualData = _gameConfig.PlayerConfig.PlayerVisualDatas.FirstOrDefault(x => x.SkinType == skinType);
            
            if (playerVisualData == null) 
                playerVisualData = _gameConfig.PlayerConfig.PlayerVisualDatas.First();
            
            _bodyRenderer.material.SetColor(ColorProperty, playerVisualData.SkinColor);
        }
    }
}