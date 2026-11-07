using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Netcode.Transports.Facepunch;
using Newtonsoft.Json;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core.Modules;
using Scripts.Runtime.Core.States;
using Scripts.Runtime.UI;
using Scripts.Runtime.Utils;
using UniRx;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using Random = UnityEngine.Random;

namespace Scripts.Runtime.Core.Net
{
    public class ConnectionManager : MonoBehaviour
    {
        public Subject<ulong> ClientConnected { get; } = new Subject<ulong>();
        public Subject<ulong> ClientDisconnected { get; } = new Subject<ulong>();

        public IObservable<Unit> StartGameRequested => _startGameRequested;
        public int PlayersCount => _networkManager != null ? _networkManager.ConnectedClients.Count : 0;
        public IReadOnlyDictionary<ulong, NetworkClient> ConnectedPlayers => _networkManager.ConnectedClients;
        public string DisconnectReason => _networkManager.DisconnectReason;
        public bool IsServer => _networkManager.IsServer;
        public ulong ServerId => NetworkManager.ServerClientId;
        public ulong LocalClientId => _networkManager.LocalClientId;

        public SessionInfo SessionInfo => _sessionInfo;
        private Subject<Unit> _startGameRequested = new Subject<Unit>();

        private UniTaskCompletionSource<bool> _connectCts;
        private NetworkManager _networkManager;
        private SessionInfo _sessionInfo;
        private bool _sceneEventSubscribed;

        private GameConfig _gameConfig;
        private ModulesFacade _modulesFacade;
        private UIController _uiController;
        private IDIResolver _iDiResolver;

        [Inject]
        private void Construct(GameConfig gameConfig, ModulesFacade modulesFacade,
            UIController uiController, IDIResolver idiResolver)
        {
            _uiController = uiController;
            _iDiResolver = idiResolver;
            _modulesFacade = modulesFacade;
            _gameConfig = gameConfig;
        }

        private void Start()
        {
            EnsureManagerCreated();

            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;

            _networkManager.NetworkConfig.ConnectionApproval = true;
            _networkManager.ConnectionApprovalCallback += HandleConnectApprovalCheck;

            var steamModule = _modulesFacade.GetSteamModule();
            steamModule.GameJoinRequested.Subscribe(HandleGameJoinRequested).AddTo(destroyCancellationToken);
            steamModule.LobbyJoinRequested.Subscribe(id => JoinLobbyById(id).Forget()).AddTo(destroyCancellationToken);
            steamModule.LobbyOwnerLeft.Subscribe(_ => HandleLobbyOwnerLeft()).AddTo(destroyCancellationToken);
        }


        private void EnsureSceneEventSubscribed()
        {
            if (_sceneEventSubscribed || _networkManager.SceneManager == null)
                return;

            _networkManager.SceneManager.OnLoadEventCompleted += HandleSceneLoadEventCompleted;
            _sceneEventSubscribed = true;
        }

        private void HandleSceneLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (sceneName != "Gameplay" || IsServer)
                return;

            _uiController.HideAllActiveMenus();
            _iDiResolver.Resolve<GameStateMachine>().Enter<GameplayState>();
        }

        public void StartGameFromLobby()
        {
            _startGameRequested.OnNext(Unit.Default);
        }

        public async UniTask<bool> StartHost(string lobbyName = null, bool lobbyFriendsOnly = true)
        {
            var steamModule = _modulesFacade.GetSteamModule();

            if (_gameConfig.ConnectionConfig.Settings.IsSteamTransport)
            {
                bool lobbyCreated = await steamModule.CreateLobbyAsync(lobbyName, _gameConfig.MaxPlayers, lobbyFriendsOnly);
                if (!lobbyCreated)
                    return false;
            }

            var startHost = _networkManager.StartHost();

            if (startHost)
            {
                EnsureSceneEventSubscribed();
                return true;
            }

            steamModule.LeaveLobby();
            return startHost;
        }

        public async UniTask<bool> JoinLobbyById(ulong lobbyId)
        {
            if (!_gameConfig.ConnectionConfig.Settings.IsSteamTransport)
                return false;

            var steamModule = _modulesFacade.GetSteamModule();
            ulong? hostSteamId = await steamModule.JoinLobbyAsync(lobbyId);

            if (!hostSteamId.HasValue)
            {
                _uiController.ShowInfoPopUp(Constants.PopUps.ErrorTitle, "Could not join the lobby. It may be closed or full.");
                return false;
            }

            bool started = await StartClientViaSteam(hostSteamId.Value);
            HandleJoinFinished(started);
            return started;
        }
        
        public void LeaveGame()
        {
            var steamModule = _modulesFacade.GetSteamModule();
            steamModule.LeaveLobby();
            steamModule.ClearRichPresence();
            _networkManager.Shutdown();
            _iDiResolver.Resolve<GameStateMachine>().Enter<LoadMainMenuState>();
        }
        
        public void KickPlayer(ulong clientId, string reason)
        {
            _networkManager.DisconnectClient(clientId, reason);
        }
        
        public void AttachSessionInfo(SessionInfo sessionInfo)
        {
            _sessionInfo = sessionInfo;

            if (!IsServer)
                RegisterMyselfData();
        }
        
        public async UniTask<bool> StartClient(string address, ushort port)
        {
            var unityTransport = (UnityTransport)_networkManager.NetworkConfig.NetworkTransport;
            unityTransport.SetConnectionData(address, port);

            return await StartClientInternal();
        }
        
        public async UniTask<bool> StartClientViaSteam(ulong hostSteamId)
        {
            var facepunchTransport = (FacepunchTransport)_networkManager.NetworkConfig.NetworkTransport;
            facepunchTransport.targetSteamId = hostSteamId;

            return await StartClientInternal();
        }

        private async UniTask<bool> StartClientInternal()
        {
            _connectCts = new UniTaskCompletionSource<bool>();

            byte[] connectionData = BuildConnectionRequest();
            _networkManager.NetworkConfig.ConnectionData = connectionData;

            _networkManager.StartClient();
            EnsureSceneEventSubscribed();
            var result = await _connectCts.Task;

            return result;
        }
      
        private ulong ParseLobbyId(string connectString)
        {
            int separatorIndex = connectString.IndexOf(':');
            string rawValue = separatorIndex >= 0 ? connectString[(separatorIndex + 1)..] : connectString;
            ulong.TryParse(rawValue, out ulong lobbyId);
            return lobbyId;
        }
        
        private void RegisterMyselfData()
        {
            if (_sessionInfo == null || _sessionInfo.AmIRegisteredMyself)
                return;
            
            var steamModule = _modulesFacade.GetSteamModule();
            var steamId = steamModule.SteamId;

            string nickname;
            
            if (steamModule.IsInitialized)
                nickname = steamModule.GetMyName();
            else
                nickname = $"Player{Random.Range(1, 100)}";
                
            _sessionInfo.RegisterMyselftDataRpc(steamId, nickname);
            _sessionInfo.AmIRegisteredMyself = true;
        }

        private byte[] BuildConnectionRequest()
        {
            var connectionRequest = new ConnectionRequest();
            connectionRequest.AppVersion = Application.version;
            connectionRequest.SteamId = _modulesFacade.GetSteamModule().IsInitialized ? _modulesFacade.GetSteamModule().SteamId : Constants.SteamIdNotAvailable;

            var serializeObject = JsonConvert.SerializeObject(connectionRequest);
            var payload = Encoding.UTF8.GetBytes(serializeObject);

            return payload;
        }

        private void EnsureManagerCreated()
        {
            if (_networkManager != null)
                return;

            _networkManager = _iDiResolver.Instantiate(_gameConfig.NetworkManagerPrefab);

            _networkManager.NetworkConfig.EnableSceneManagement = true;

            if (_gameConfig.ConnectionConfig.Settings.IsSteamTransport)
            {
                var facepunchTransport = _networkManager.GetComponent<FacepunchTransport>();
                if (facepunchTransport == null)
                    facepunchTransport = _networkManager.gameObject.AddComponent<FacepunchTransport>();

                _networkManager.NetworkConfig.NetworkTransport = facepunchTransport;
            }
        }
        
        private void HandleGameJoinRequested(string connectString)
        {
            ulong id = ParseLobbyId(connectString);
            if (id == 0)
                return;

            var steamModule = _modulesFacade.GetSteamModule();

            if (steamModule.CurrentLobby.HasValue && steamModule.CurrentLobby.Value.Id.Value == id)
                return;

            JoinLobbyById(id).Forget();
        }
        
        private void HandleConnectApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (ServerId == request.ClientNetworkId)
            {
                response.Approved = true;
                return;
            }
            
            response.CreatePlayerObject = false;
            
            var json = Encoding.UTF8.GetString(request.Payload);
            var connectionRequest = JsonConvert.DeserializeObject<ConnectionRequest>(json);

            if (connectionRequest.SteamId != Constants.SteamIdNotAvailable)
            {
                if (!_gameConfig.ConnectionConfig.DisableSteamUserCheck && SessionInfo.IsSteamUserAlreadyExist(connectionRequest.SteamId))
                {
                    Refuse("This Steam user is already connected to a server.");
                    return;
                }
            }
            
            if (connectionRequest.AppVersion != Application.version)
            {
                Refuse("Game versions mismatch.");
                return;
            }

            if (PlayersCount >= _gameConfig.MaxPlayers)
            {
                Refuse("Max players reached.");
                return;
            }

            void Refuse(string reason)
            {
                response.Approved = false;
                response.Reason = reason;
            }
            
            response.Approved = true;
        }
        
        private void SyncLobbyWithSession()
        {
            var steamModule = _modulesFacade.GetSteamModule();

            if (!steamModule.IsInitialized)
                return;

            bool inGameplay = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Gameplay";

            steamModule.SetStatus(inGameplay ? "In Game" : "In Lobby");

            if (IsServer)
                steamModule.UpdateLobbyState(PlayersCount, _gameConfig.MaxPlayers, inGameplay);
        }
        
        private void HandleJoinFinished(bool started)
        {
            if (!started)
            {
                _uiController.ShowInfoPopUp(Constants.PopUps.ErrorTitle, $"Error connecting to the server: {DisconnectReason}");
                _modulesFacade.GetSteamModule().LeaveLobby();
                return;
            }

            // Late join: the game is already in progress, go straight to gameplay instead of the lobby.
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Gameplay")
            {
                _uiController.HideAllActiveMenus();
                _iDiResolver.Resolve<GameStateMachine>().Enter<GameplayState>();
                return;
            }

            _uiController.HideMenu<MainMenu>();
            _uiController.ShowMenu<LobbyMenu>();
        }
        
        private void HandleLobbyOwnerLeft()
        {
            if (!_networkManager.IsConnectedClient && !_networkManager.IsServer)
                return;

            LeaveGame();
            _uiController.FlushGameplayMenus();
            _uiController.ShowInfoPopUp(Constants.PopUps.InfoTitle, "The host has closed the lobby.");
        }
        
        private void HandleClientConnected(ulong clientId)
        {
            if (IsServer && clientId == ServerId) // Game hosted
            {
                _sessionInfo = _iDiResolver.Instantiate(_gameConfig.SessionInfoPrefab);
                _sessionInfo.GetComponent<NetworkObject>().Spawn();
                DontDestroyOnLoad(_sessionInfo.gameObject);
                RegisterMyselfData();

                var steamModule = _modulesFacade.GetSteamModule();
                if (steamModule.IsInitialized)
                {
                    ulong joinId = steamModule.CurrentLobby.HasValue ? steamModule.CurrentLobby.Value.Id.Value : steamModule.SteamId;
                    steamModule.SetJoinString(joinId.ToString());
                }
            }

            if (!IsServer && clientId == LocalClientId)
            {
                RegisterMyselfData();
            }
            
            _connectCts?.TrySetResult(true);
            ClientConnected.OnNext(clientId);
            SyncLobbyWithSession();
        }
        
        private void HandleClientDisconnected(ulong clientId)
        {
            if (_networkManager.IsServer)
            {
                SessionInfo.HandleClientDisconnect(clientId);
            }
            else if (clientId == LocalClientId)
            {
                LeaveGame();

                _uiController.FlushGameplayMenus();
                _uiController.ShowInfoPopUp(Constants.PopUps.InfoTitle, $"Disconnected from server: {DisconnectReason}");
            }
            
            _connectCts?.TrySetResult(false);
            ClientDisconnected.OnNext(clientId);
            SyncLobbyWithSession();
        }

        private void OnDestroy()
        {
            if (_sceneEventSubscribed && _networkManager != null && _networkManager.SceneManager != null)
                _networkManager.SceneManager.OnLoadEventCompleted -= HandleSceneLoadEventCompleted;

            _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            _networkManager.ConnectionApprovalCallback -= HandleConnectApprovalCheck;
        }
    }

    public class ConnectionRequest
    {
        public string AppVersion;
        public ulong SteamId;
    }
}