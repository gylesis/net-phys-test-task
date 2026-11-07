using System;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core;
using Scripts.Runtime.Core.Modules;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Scripts.Runtime.UI
{
    public class GameplayPauseMenu : Menu
    {
        private const float PingUpdateInterval = 1f;

        [SerializeField] private DefaultReactiveButton _exitBtn;
        [SerializeField] private DefaultReactiveButton _resumeBtn;
        [SerializeField] private SessionPlayersView _sessionPlayersView;
        [SerializeField] private TMP_Dropdown _skinDropdown;
        [SerializeField] private TMP_Text _steamLobbyIdText;
        
        private IDisposable _updatePingDisposable;
        private GameConfig _gameConfig;
        private ConnectionManager _connectionManager;
        private UIController _uiController;
        private LocalPlayerContext _localPlayerContext;
        private ModulesFacade _modulesFacade;

        private SessionInfo SessionInfo => _connectionManager.SessionInfo;

        protected override void Awake()
        {
            base.Awake();

            _resumeBtn.Clicked.Subscribe(unit => HandleResumeBtnClicked()).AddTo(destroyCancellationToken);
            _exitBtn.Clicked.Subscribe(unit => HandleExitBtnClicked()).AddTo(destroyCancellationToken);

            _skinDropdown.onValueChanged.AddListener(HandleSkinDropdownChanged);
        }

        private void OnDestroy()
        {
            _skinDropdown.onValueChanged.RemoveAllListeners();
        }

        [Inject]
        private void Construct(GameConfig gameConfig, ConnectionManager connectionManager, UIController uiController,
            LocalPlayerContext localPlayerContext, ModulesFacade modulesFacade)
        {
            _localPlayerContext = localPlayerContext;
            _uiController = uiController;
            _connectionManager = connectionManager;
            _gameConfig = gameConfig;
            _modulesFacade = modulesFacade;
        }

        protected override void Start()
        {
            base.Start();
            _connectionManager.ClientConnected.Subscribe(obj => UpdatePlayers()).AddTo(destroyCancellationToken);
            _connectionManager.ClientDisconnected.Subscribe(obj => UpdatePlayers()).AddTo(destroyCancellationToken);
        }

        public override void Show()
        {
            UpdatePlayers();
            UpdateSkinDropdown();
            UpdateSteamLobbyId();
            _updatePingDisposable = Observable.Interval(TimeSpan.FromSeconds(PingUpdateInterval)).Subscribe(l => UpdatePlayersPing());
            base.Show();
        }
        
        public override void Hide()
        {
            base.Hide();
            _updatePingDisposable?.Dispose();
        }

        private void UpdatePlayers()
        {
            ListPool<PlayerInfo>.Get(out var playerInfos);
            ListPool<ushort>.Get(out var pings);
            for (var index = 0; index < SessionInfo.PlayersInfo.Count; index++)
            {
                playerInfos.Add(SessionInfo.PlayersInfo[index]);
                pings.Add(index < SessionInfo.PlayersPing.Count ? SessionInfo.PlayersPing[index] : (ushort)0);
            }

            _sessionPlayersView.UpdatePlayers(playerInfos, pings);
        }
        
        private void UpdateSkinDropdown()
        {
            FillSkinDropdownOptions();

            var currentSkinIndex = GetLocalPlayerSkinIndex();
            _skinDropdown.SetValueWithoutNotify(currentSkinIndex);
            _skinDropdown.RefreshShownValue();
        }

        private void FillSkinDropdownOptions()
        {
            if (_skinDropdown.options.Count == _gameConfig.PlayerConfig.PlayerVisualDatas.Count)
                return;

            _skinDropdown.ClearOptions();
            foreach (var playerVisualData in _gameConfig.PlayerConfig.PlayerVisualDatas)
                _skinDropdown.options.Add(new TMP_Dropdown.OptionData(playerVisualData.SkinType.ToString()));

            _skinDropdown.RefreshShownValue();
        }

        private int GetLocalPlayerSkinIndex()
        {
            var localPlayer = _localPlayerContext.Current;
            if (localPlayer == null)
                return 0;

            foreach (var playerInfo in SessionInfo.PlayersInfo)
            {
                if (playerInfo.ClientId == localPlayer.OwnerClientId)
                    return playerInfo.SkinIndex;
            }

            return 0;
        }

        private void UpdateSteamLobbyId()
        {
            var steamModule = _modulesFacade.GetSteamModule();

            if (!steamModule.IsInitialized || !steamModule.CurrentLobby.HasValue)
            {
                _steamLobbyIdText.gameObject.SetActive(false);
                return;
            }

            _steamLobbyIdText.gameObject.SetActive(true);
            _steamLobbyIdText.text = $"Steam Lobby ID: {steamModule.CurrentLobby.Value.Id.Value}";
        }

        private void UpdatePlayersPing()
        {
            ListPool<ushort>.Get(out var pings);
            foreach (var ping in SessionInfo.PlayersPing)
                pings.Add(ping);

            _sessionPlayersView.UpdatePing(pings);
        }

        private void HandleSkinDropdownChanged(int index)
        {
            var playerVisualDatas = _gameConfig.PlayerConfig.PlayerVisualDatas;
            if (index < 0 || index >= playerVisualDatas.Count)
                return;

            SessionInfo.ChangeSkinRpc(playerVisualDatas[index].SkinType);
        }

        private void HandleExitBtnClicked()
        {
            _connectionManager.LeaveGame();
        }

        private void HandleResumeBtnClicked()
        {
            _uiController.HideMenu<GameplayPauseMenu>();
            _uiController.ShowMenu<GameplayHUDMenu>();

            _localPlayerContext.Current.IsPaused.Value = false;
        }
    }
}
