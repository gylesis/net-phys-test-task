using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core;
using Scripts.Runtime.Core.Modules;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Utils;
using UniRx;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.UI
{
    public class SessionPlayersView : MonoBehaviour
    {
        [SerializeField] private Transform _playersViewParent;

        private List<SessionPlayerView> _views = new List<SessionPlayerView>();

        private GameConfig _gameConfig;
        private ModulesFacade _modulesFacade;
        private ConnectionManager _connectionManager;

        [Inject]
        private void Construct(GameConfig gameConfig, ModulesFacade modulesFacade, ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _gameConfig = gameConfig;
            _modulesFacade = modulesFacade;
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
        
        public void UpdatePlayers(List<PlayerInfo> playerInfos, List<ushort> pings)
        {
            TrySpawnViews();
            _views.ForEach(x => x.gameObject.SetActive(false));

            for (var index = 0; index < playerInfos.Count; index++)
            {
                var playerInfo = playerInfos[index];
                SessionPlayerView sessionPlayerView = _views[index];

                sessionPlayerView.SetKickBtnEnabled(_connectionManager.IsServer && playerInfo.ClientId != _connectionManager.ServerId);
                
                sessionPlayerView.ClientId = playerInfo.ClientId;
                sessionPlayerView.SetPing($"{pings[index]}");
                SetNickname(sessionPlayerView, playerInfo);
                
                sessionPlayerView.gameObject.SetActive(true);
            }
        }

        private void SetNickname(SessionPlayerView view, PlayerInfo playerInfo)
        {
            view.SetNickname(playerInfo.Name.Value);
            ResolveNickname(view, playerInfo).Forget();
        }

        private async UniTaskVoid ResolveNickname(SessionPlayerView view, PlayerInfo playerInfo)
        {
            var steamModule = _modulesFacade.GetSteamModule();

            if (!steamModule.IsInitialized || playerInfo.SteamID == Constants.SteamIdNotAvailable)
                return;

            string steamName = await steamModule.GetSteamNameCachedAsync(playerInfo.SteamID, destroyCancellationToken);

            if (string.IsNullOrEmpty(steamName))
                return;

            if (view != null && view.ClientId == playerInfo.ClientId)
                view.SetNickname(steamName);
        }

        public void UpdatePing(List<ushort> pings)
        {
            for (var index = 0; index < pings.Count; index++)
            {
                SessionPlayerView sessionPlayerView = _views[index];
                sessionPlayerView.SetPing($"{pings[index]}");
            }
        }
        
        private void HandleKickBtnClicked(SessionPlayerView sessionPlayerView)
        {
            _connectionManager.KickPlayer(sessionPlayerView.ClientId, "Kicked");
        }
    }
}