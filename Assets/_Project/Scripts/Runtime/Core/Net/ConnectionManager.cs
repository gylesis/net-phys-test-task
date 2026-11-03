using System;
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
        
        private GameConfig _gameConfig;
        private NetworkManager _networkManager;

        public Subject<ulong> ClientConnected { get; } = new Subject<ulong>();
        public Subject<ulong> ClientDisconnected { get; } = new Subject<ulong>();

        public int PlayersCount => _networkManager != null ? _networkManager.ConnectedClients.Count : 0;
        
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
        }

        private void OnDestroy()
        {
            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        private void HandleClientConnected(ulong obj)
        {
            ClientConnected.OnNext(obj);
        }

        private void HandleClientDisconnected(ulong obj)
        {
            ClientDisconnected.OnNext(obj);
        }

        public bool StartHost()
        {
            return _networkManager.StartHost();
        }
        
        public bool StartClient(string address, ushort port)
        {
            var unityTransport = _networkManager.GetComponent<UnityTransport>();
            unityTransport.SetConnectionData(address, port);
            return _networkManager.StartClient();
        }

        private void EnsureManagerCreated()
        {
            if (_networkManager == null) 
                _networkManager = Instantiate(_gameConfig.NetworkManagerPrefab);
        }
    }
}