using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Utils;
using UniRx;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.UI
{
    [RequireComponent(typeof(AutoInjecter))]
    public class SessionPlayersView : MonoBehaviour
    {
        [SerializeField] private Transform _playersViewParent;
        private ConnectionManager ConnectionManager => ConnectionManager.Instance;
        
        private List<SessionPlayerView> _views = new List<SessionPlayerView>();
        
        private GameConfig _gameConfig;

        [Inject]
        private void Construct(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }
        
        private void TrySpawnViews()
        {
            if (_views.Count != 0)
                return;
            
            for (int i = 0; i < _gameConfig.MaxPlayers; i++)
            {
                SessionPlayerView sessionPlayerView = Instantiate(_gameConfig.UIConfig.SessionPlayerViewPrefab, _playersViewParent);
                sessionPlayerView.gameObject.SetActive(false);
                sessionPlayerView.KickBtnClicked.Subscribe(unit => HandleKickBtnClicked(sessionPlayerView)).AddTo(destroyCancellationToken);
                
                _views.Add(sessionPlayerView);
            }
        }

        private void HandleKickBtnClicked(SessionPlayerView sessionPlayerView)
        {
            ConnectionManager.KickPlayer(sessionPlayerView.ClientId, "Kicked");
        }

        public void UpdatePlayers(List<PlayerInfo> playerInfos)
        {
            TrySpawnViews();
            _views.ForEach(x => x.gameObject.SetActive(false));

            for (var index = 0; index < playerInfos.Count; index++)
            {
                var playerInfo = playerInfos[index];
                SessionPlayerView sessionPlayerView = _views[index];

                sessionPlayerView.SetKickBtnEnabled(ConnectionManager.IsServer && playerInfo.ClientId != ConnectionManager.ServerId);
                
                sessionPlayerView.ClientId = playerInfo.ClientId;
                sessionPlayerView.SetNickname(playerInfo.Name.Value);
                sessionPlayerView.SetPing($"{playerInfo.LastPing}");
                
                sessionPlayerView.gameObject.SetActive(true);
            }
        }

        public void UpdatePing(List<PlayerInfo> playerInfos)
        {   
            for (var index = 0; index < playerInfos.Count; index++)
            {
                var playerInfo = playerInfos[index];
                SessionPlayerView sessionPlayerView = _views[index];

                var playerRtt = playerInfo.LastPing;
                sessionPlayerView.SetPing($"{playerRtt}");
            }
        }
    }
}