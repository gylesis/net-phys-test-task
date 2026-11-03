using System;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using UniRx;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.Core.Net
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }
        public Subject<ulong> ClientConnected { get; } = new Subject<ulong>();
        public Subject<ulong> ClientDisconnected { get; } = new Subject<ulong>();

        public int PlayersCount => _networkManager != null ? _networkManager.ConnectedClients.Count : 0;
        public string DisconnectReason => _networkManager.DisconnectReason;
        public bool IsServer => _networkManager.IsServer;
        
        private UniTaskCompletionSource<bool> _connectCts;
        private GameConfig _gameConfig;
        private NetworkManager _networkManager;
        
        private void Awake()
        {
            Instance = this;
        }

        [Inject]
        private void Construct(GameConfig gameConfig)
        {
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

        private void HandleConnectApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.CreatePlayerObject = false;
            
            if (PlayersCount >= _gameConfig.MaxPlayers)
            {
                response.Approved = false;
                response.Reason = "Max players reached.";
                return;
            }
            
            response.Approved = true;
        }

        public bool StartHost()
        {
            return _networkManager.StartHost();
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

        private byte[] BuildPayload()
        {
            return Array.Empty<byte>();
        }

        private void EnsureManagerCreated()
        {
            if (_networkManager == null) 
                _networkManager = Instantiate(_gameConfig.NetworkManagerPrefab);
        }
        
        private void HandleClientConnected(ulong obj)
        {
            _connectCts?.TrySetResult(true);
            ClientConnected.OnNext(obj);
        }

        private void HandleClientDisconnected(ulong obj)
        {
            _connectCts?.TrySetResult(false);
            ClientDisconnected.OnNext(obj);
        }

        private void OnDestroy()
        {
            _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            _networkManager.ConnectionApprovalCallback -= HandleConnectApprovalCheck;
        }
    }
}