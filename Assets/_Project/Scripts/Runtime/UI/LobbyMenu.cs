using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core;
using Scripts.Runtime.Core.Modules;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Utils;
using TMPro;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.UI
{
    public class LobbyMenu : Menu
    {
        [SerializeField] private DefaultTextReactiveButton _startGameBtn;
        [SerializeField] private TMP_Text _playersCountText;
        [SerializeField] private TMP_Text _playersListText;
        [SerializeField] private TMP_Text _steamLobbyIdText;
        [SerializeField] private DefaultTextReactiveButton _inviteBtn;
        
        private GameConfig _gameConfig;
        private ModulesFacade _modulesFacade;
        private ConnectionManager _connectionManager;

        protected override void Awake()
        {
            base.Awake();
            _startGameBtn.Clicked.Subscribe(unit => HandleStartBtnClicked()).AddTo(destroyCancellationToken);
            _inviteBtn.Clicked.Subscribe(unit => HandleInviteBtnClicked()).AddTo(destroyCancellationToken);
        }

        [Inject]
        private void Construct(GameConfig gameConfig, ModulesFacade modulesFacade, ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
            _gameConfig = gameConfig;
            _modulesFacade = modulesFacade;

            _connectionManager.ClientConnected.Subscribe(HandleClientConnected).AddTo(destroyCancellationToken);
            _connectionManager.ClientDisconnected.Subscribe(HandleClientDisconnected).AddTo(destroyCancellationToken);
        }

        protected override void Start()
        {
            base.Start();
            _inviteBtn.gameObject.SetActive(_gameConfig.ConnectionConfig.Settings.IsSteamTransport);
        }

        public override void Show()
        {
            UpdatePlayersCount();
            _startGameBtn.gameObject.SetActive(_connectionManager.IsServer);

            var sessionInfo = _connectionManager.SessionInfo;

            if (sessionInfo != null)
            {
                sessionInfo.PlayersInfo.OnListChanged -= HandlePlayersInfoChanged;
                sessionInfo.PlayersInfo.OnListChanged += HandlePlayersInfoChanged;
            }

            UpdatePlayersList();
            UpdateSteamLobbyId();

            base.Show();
        }

        public override void Hide()
        {
            var sessionInfo = _connectionManager.SessionInfo;

            if (sessionInfo != null)
                sessionInfo.PlayersInfo.OnListChanged -= HandlePlayersInfoChanged;

            base.Hide();
        }

        private void HandlePlayersInfoChanged(NetworkListEvent<PlayerInfo> change)
        {
            UpdatePlayersCount();
            UpdatePlayersList();
        }

        private void UpdatePlayersList()
        {
            var sessionInfo = _connectionManager.SessionInfo;

            if (sessionInfo == null)
            {
                _playersListText.text = string.Empty;
                return;
            }

            var builder = new System.Text.StringBuilder();

            foreach (var playerInfo in sessionInfo.PlayersInfo)
            {
                if (builder.Length > 0)
                    builder.AppendLine();

                builder.Append(playerInfo.Name.Value);
            }

            _playersListText.text = builder.ToString();
        }

        private void UpdateSteamLobbyId()
        {
            if (_steamLobbyIdText == null)
                return;

            var steamModule = _modulesFacade.GetSteamModule();

            if (!steamModule.IsInitialized || !steamModule.CurrentLobby.HasValue)
            {
                _steamLobbyIdText.gameObject.SetActive(false);
                return;
            }

            _steamLobbyIdText.gameObject.SetActive(true);
            _steamLobbyIdText.text = $"Steam Lobby ID: {steamModule.CurrentLobby.Value.Id.Value}";
        }

        private void UpdatePlayersCount()
        {
            _playersCountText.text = $"Players: {_connectionManager.PlayersCount} / {_gameConfig.MaxPlayers}";
        }

        private void HandleClientConnected(ulong obj)
        {
            UpdatePlayersCount();
            UpdatePlayersList();
        }

        private void HandleClientDisconnected(ulong obj)
        {
            UpdatePlayersCount();
            UpdatePlayersList();
        }

        private void HandleStartBtnClicked()
        {
            _connectionManager.StartGameFromLobby();
        }

        private void HandleInviteBtnClicked()
        {
            _modulesFacade.GetSteamModule().OpenLobbyInviteOverlay();
        }

    }
}
