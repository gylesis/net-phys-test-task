using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.States;
using Scripts.Runtime.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Scripts.Runtime.UI
{
    public class MainMenu : Menu
    {
        [SerializeField] private DefaultTextReactiveButton _hostBtn;
        [SerializeField] private DefaultTextReactiveButton _joinBtn;

        [SerializeField] private TMP_InputField _ipInput;
        [SerializeField] private TMP_InputField _portInput;

        [SerializeField] private TMP_InputField _lobbyNameInput;
        [SerializeField] private TMP_InputField _lobbyIdInput;
        [SerializeField] private DefaultTextReactiveButton _visibilityBtn;

        [SerializeField] private CanvasGroup _steamContainer;
        [SerializeField] private TMP_Text _steamStatus;
        
        private GameStateMachine _gameStateMachine;
        private GameConfig _gameConfig;
        private ConnectionManager _connectionManager;
        private UIController _uiController;

        private bool _lobbyFriendsOnly = true;

        protected override void Awake()
        {
            base.Awake();

            _hostBtn.Clicked.Subscribe(unit => HandleHostBtnClicked()).AddTo(destroyCancellationToken);
            _joinBtn.Clicked.Subscribe(unit => HandleJoinBtnClicked()).AddTo(destroyCancellationToken);
        }

        [Inject]
        private void Construct(GameStateMachine gameStateMachine, GameConfig gameConfig,
            ConnectionManager connectionManager, UIController uiController)
        {
            _uiController = uiController;
            _connectionManager = connectionManager;
            _gameStateMachine = gameStateMachine;
            _gameConfig = gameConfig;
        }

        protected override void Start()
        {
            base.Start();
            _visibilityBtn.Clicked.Subscribe(_ => UpdateLobbyVisibility(!_lobbyFriendsOnly)).AddTo(destroyCancellationToken);
            UpdateLobbyVisibility(_lobbyFriendsOnly);

            var isSteamTransport = _gameConfig.ConnectionConfig.Settings.IsSteamTransport;
            _steamStatus.gameObject.SetActive(!isSteamTransport);
            _steamStatus.text = isSteamTransport ? "Steam is available." : "Steam is not available.";
            _steamContainer.alpha = isSteamTransport ? 1 : 0.6f;
            _steamContainer.interactable = false;
            
            _ipInput.gameObject.SetActive(!isSteamTransport);
            _portInput.gameObject.SetActive(!isSteamTransport);
        }

        private void UpdateLobbyVisibility(bool isVisible)
        {
            _lobbyFriendsOnly = isVisible;
            _visibilityBtn.SetText(isVisible ? "Friends Only" : "Private");
        }

        private void HandleJoinBtnClicked()
        {
            TryJoin().Forget();
        }

        private async UniTask TryJoin()
        {
            bool started;

            if (_gameConfig.ConnectionConfig.Settings.IsSteamTransport)
            {
                started = await TryJoinSteam();
            }
            else
            {
                if (!ushort.TryParse(_portInput.text, out ushort port))
                    return;

                DisableCanvasGroup();
                started = await _connectionManager.StartClient(_ipInput.text, port);
            }

            HandleJoinResult(started);
        }

        private async UniTask<bool> TryJoinSteam()
        {
            if (_lobbyIdInput != null && ulong.TryParse(_lobbyIdInput.text, out ulong lobbyId) && lobbyId != 0)
            {
                DisableCanvasGroup();
                return await _connectionManager.JoinLobbyById(lobbyId);
            }

            if (!ulong.TryParse(_ipInput.text, out ulong hostSteamId))
                return false;

            DisableCanvasGroup();
            return await _connectionManager.StartClientViaSteam(hostSteamId);
        }

        private void HandleJoinResult(bool started)
        {
            EnableCanvasGroup();

            if (!started)
            {
                _uiController.ShowInfoPopUp(Constants.PopUps.ErrorTitle, $"Error connecting to the server: {_connectionManager.DisconnectReason}");
                return;
            }

            _uiController.HideMenu<MainMenu>();

            if (SceneManager.GetActiveScene().name == "Gameplay")
            {
                _gameStateMachine.Enter<GameplayState>();
            }
            else
            {
                _uiController.ShowMenu<LobbyMenu>();
            }
        }

        private void HandleHostBtnClicked()
        {
            TryHost().Forget();
        }

        private async UniTask TryHost()
        {
            string lobbyName = _lobbyNameInput != null ? _lobbyNameInput.text : null;
            bool hosted = await _connectionManager.StartHost(lobbyName, _lobbyFriendsOnly);

            if (hosted)
            {
                _uiController.HideMenu<MainMenu>();
                _uiController.ShowMenu<LobbyMenu>();
            }
        }
    }
}
