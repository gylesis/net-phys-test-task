using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
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

namespace Scripts.Runtime.Core.Net
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }

        public Subject<ulong> ClientConnected { get; } = new Subject<ulong>();
        public Subject<ulong> ClientDisconnected { get; } = new Subject<ulong>();

        public int PlayersCount => _networkManager != null ? _networkManager.ConnectedClients.Count : 0;

        public IReadOnlyDictionary<ulong, NetworkClient> ConnectedPlayers => _networkManager.ConnectedClients;
        public string DisconnectReason => _networkManager.DisconnectReason;
        public bool IsServer => _networkManager.IsServer;
        public ulong ServerId => NetworkManager.ServerClientId;
        public ulong LocalClientId => _networkManager.LocalClientId;
        public SessionInfo SessionInfo => SessionInfo.Instance;

        private UniTaskCompletionSource<bool> _connectCts;
        private NetworkManager _networkManager;

        private GameConfig _gameConfig;
        private GameStateMachine _gameStateMachine;
        private ModulesFacade _modulesFacade;
        
        private void Awake()
        {
            Instance = this;
        }

        [Inject]
        private void Construct(GameConfig gameConfig, GameStateMachine gameStateMachine, ModulesFacade modulesFacade)
        {
            _modulesFacade = modulesFacade;
            _gameStateMachine = gameStateMachine;
            _gameConfig = gameConfig;
        }

        private void Start()
        {
            EnsureManagerCreated();

            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            
            _networkManager.NetworkConfig.ConnectionApproval = true;
            _networkManager.ConnectionApprovalCallback += HandleConnectApprovalCheck;
        }

        public void StartGameFromLobby()
        {
            var mainMenuNetController = DiInjecter.Instance.Resolve<MainMenuNetController>();
            mainMenuNetController.StartLoadGameplayStateRpc();
        }

        public bool StartHost()
        {
            var startHost = _networkManager.StartHost();
            return startHost;
        }
        
        public async UniTask<bool> StartClient(string address, ushort port)
        {
            _connectCts = new UniTaskCompletionSource<bool>();
            
            var unityTransport = _networkManager.GetComponent<UnityTransport>();
            unityTransport.SetConnectionData(address, port);
            
            byte[] connectionData = BuildPayload();
            _networkManager.NetworkConfig.ConnectionData = connectionData;
            
            _networkManager.StartClient();
            var result = await _connectCts.Task;

            return result;
        }
        
        public void LeaveGame()
        {
            _networkManager.Shutdown();
            _gameStateMachine.Enter<LoadMainMenuState>();
        }
        
        public void KickPlayer(ulong clientId, string reason)
        {
            _networkManager.DisconnectClient(clientId, reason);
        }
        
        private void RegisterMyselfData()
        {
            if (SessionInfo.AmIRegisteredMyself)
                return;
            
            var steamModule = _modulesFacade.GetSteamModule();
            var steamId = steamModule.SteamId;

            string nickname;
            
            if (steamModule.IsInitialized)
                nickname = steamModule.GetMyName();
            else
                nickname = $"Player{Random.Range(1, 100)}";
                
            SessionInfo.RegisterMyselftDataRpc(steamId, nickname);
            SessionInfo.AmIRegisteredMyself = true;
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
                if (SessionInfo.IsSteamUserAlreadyExist(connectionRequest.SteamId))
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

        private byte[] BuildPayload()
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
            if (_networkManager == null) 
                _networkManager = Instantiate(_gameConfig.NetworkManagerPrefab);
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (IsServer && clientId == ServerId) // Game hosted
            {
                var sessionInfo = Instantiate(_gameConfig.SessionInfoPrefab);
                sessionInfo.GetComponent<NetworkObject>().Spawn();
                sessionInfo.GetComponent<SessionInfo>();
                DontDestroyOnLoad(sessionInfo.gameObject);
                RegisterMyselfData();
            }

            if (!IsServer && clientId == LocalClientId)
            {
                RegisterMyselfData();
            }
            
            _connectCts?.TrySetResult(true);
            ClientConnected.OnNext(clientId);
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
                
                UIController.Instance.FlushGameplayMenus();
                UIController.Instance.ShowInfoPopUp(Constants.PopUps.InfoTitle, $"Disconnected from server: {DisconnectReason}");
            }
            
            _connectCts?.TrySetResult(false);
            ClientDisconnected.OnNext(clientId);
        }

        private void OnDestroy()
        {
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