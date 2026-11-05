using System;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core;
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
    [RequireComponent(typeof(AutoInjecter))]
    public class GameplayPauseMenu : Menu
    {
        private const float PingUpdateInterval = 1f;
        
        [SerializeField] private DefaultReactiveButton _exitBtn;
        [SerializeField] private DefaultReactiveButton _resumeBtn;
        [SerializeField] private SessionPlayersView _sessionPlayersView;
        [SerializeField] private TMP_Dropdown _skinDropdown;
        
        private SessionInfo SessionInfo => ConnectionManager.Instance.SessionInfo;
        
        private IDisposable _updatePingDisposable;
        private GameConfig _gameConfig;

        protected override void Awake()
        {
            base.Awake();

            _resumeBtn.Clicked.Subscribe(unit => HandleResumeBtnClicked()).AddTo(destroyCancellationToken);
            _exitBtn.Clicked.Subscribe(unit => HandleExitBtnClicked()).AddTo(destroyCancellationToken);

            ConnectionManager.Instance.ClientConnected.Subscribe(obj => UpdatePlayers()).AddTo(destroyCancellationToken);
            ConnectionManager.Instance.ClientDisconnected.Subscribe(obj => UpdatePlayers()).AddTo(destroyCancellationToken);

            _skinDropdown.onValueChanged.AddListener(HandleSkinDropdownChanged);
        }

        private void OnDestroy()
        {
            _skinDropdown.onValueChanged.RemoveAllListeners();
        }

        [Inject]
        private void Construct(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
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

        public override void Show()
        {
            UpdatePlayers();
            UpdateSkinDropdown();
            _updatePingDisposable = Observable.Interval(TimeSpan.FromSeconds(PingUpdateInterval)).Subscribe(l => UpdatePlayersPing());
            base.Show();
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
            var localPlayer = PlayerController.LocalPlayer;
            if (localPlayer == null)
                return 0;

            foreach (var playerInfo in SessionInfo.PlayersInfo)
            {
                if (playerInfo.ClientId == localPlayer.OwnerClientId)
                    return playerInfo.SkinIndex;
            }

            return 0;
        }

        private void HandleSkinDropdownChanged(int index)
        {
            if (_gameConfig == null)
                return;

            var playerVisualDatas = _gameConfig.PlayerConfig.PlayerVisualDatas;
            if (index < 0 || index >= playerVisualDatas.Count)
                return;

            SessionInfo.ChangeSkinRpc(playerVisualDatas[index].SkinType);
        }

        public override void Hide()
        {
            base.Hide();
            _updatePingDisposable?.Dispose();
        }

        private void UpdatePlayersPing()
        {
            ListPool<ushort>.Get(out var pings);
            foreach (var ping in SessionInfo.PlayersPing)
                pings.Add(ping);
                
            _sessionPlayersView.UpdatePing(pings);
        }

        private void HandleExitBtnClicked()
        {
            ConnectionManager.Instance.LeaveGame();
        }

        private void HandleResumeBtnClicked()
        {
            UIController.Instance.HideMenu<GameplayPauseMenu>();
            UIController.Instance.ShowMenu<GameplayHUDMenu>();
            
            PlayerController.LocalPlayer.IsPaused.Value = false;
        }
    }
}