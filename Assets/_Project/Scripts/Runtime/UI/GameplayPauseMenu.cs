using System;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Core;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;

namespace Scripts.Runtime.UI
{
    public class GameplayPauseMenu : Menu
    {
        private const float PingUpdateInterval = 1f;
        
        [SerializeField] private DefaultReactiveButton _exitBtn;
        [SerializeField] private DefaultReactiveButton _resumeBtn;
        [SerializeField] private SessionPlayersView _sessionPlayersView;
        
        private IDisposable _updatePingDisposable;

        protected override void Awake()
        {
            base.Awake();

            _resumeBtn.Clicked.Subscribe(unit => HandleResumeBtnClicked()).AddTo(destroyCancellationToken);
            _exitBtn.Clicked.Subscribe(unit => HandleExitBtnClicked()).AddTo(destroyCancellationToken);

            ConnectionManager.Instance.ClientConnected.Subscribe(obj => UpdatePlayers()).AddTo(destroyCancellationToken);
            ConnectionManager.Instance.ClientDisconnected.Subscribe(obj => UpdatePlayers()).AddTo(destroyCancellationToken);
        }

        private void UpdatePlayers()
        {
            ListPool<PlayerInfo>.Get(out var playerInfos);
            foreach (var playerInfo in ConnectionManager.Instance.SessionInfo.PlayersInfo) 
                playerInfos.Add(playerInfo);
            
            _sessionPlayersView.UpdatePlayers(playerInfos);
        }

        public override void Show()
        {
            UpdatePlayers();
            _updatePingDisposable = Observable.Interval(TimeSpan.FromSeconds(PingUpdateInterval)).Subscribe(l => UpdatePlayersPing());
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
            _updatePingDisposable?.Dispose();
        }

        private void UpdatePlayersPing()
        {
            ListPool<PlayerInfo>.Get(out var playerInfos);
            foreach (var playerInfo in ConnectionManager.Instance.SessionInfo.PlayersInfo) 
                playerInfos.Add(playerInfo);
                
            _sessionPlayersView.UpdatePing(playerInfos);
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