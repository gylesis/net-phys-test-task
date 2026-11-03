using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.States;
using Scripts.Runtime.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.UI
{
    [RequireComponent(typeof(AutoInjecter))]
    public class LobbyMenu : Menu
    {
        [SerializeField] private DefaultTextReactiveButton _startGameBtn;
        [SerializeField] private TMP_Text _playersCountText;
        
        private GameStateMachine _gameStateMachine;
        private GameConfig _gameConfig;

        protected override void Awake()
        {
            base.Awake();
            _startGameBtn.Clicked.Subscribe(unit => HandleStartBtnClicked()).AddTo(destroyCancellationToken);

            ConnectionManager.Instance.ClientConnected.Subscribe(HandleClientConnected).AddTo(destroyCancellationToken);
            ConnectionManager.Instance.ClientDisconnected.Subscribe(HandleClientDisconnected).AddTo(destroyCancellationToken);
            
            UpdatePlayersCount();
        }
        
        [Inject]
        private void Construct(GameStateMachine gameStateMachine, GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
            _gameStateMachine = gameStateMachine;
        }

        private void UpdatePlayersCount()
        {
            _playersCountText.text = $"Players: {ConnectionManager.Instance.PlayersCount} / {_gameConfig.MaxPlayers}";
        }

        private void HandleClientConnected(ulong obj)
        {
            UpdatePlayersCount();
        }
        
        private void HandleClientDisconnected(ulong obj)
        {
            UpdatePlayersCount();
        }
        
        private void HandleStartBtnClicked()
        {
            UIController.Instance.HideMenu<LobbyMenu>();
            _gameStateMachine.Enter<LoadGameplayState>();
        }
        
    }
}