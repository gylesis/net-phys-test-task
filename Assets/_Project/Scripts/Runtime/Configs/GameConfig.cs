using Scripts.Runtime.Core;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Runtime.Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "NetPhys/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Session")]
        [SerializeField, Range(1, 4)] private int _maxPlayers = 4;
        [SerializeField] private NetworkManager _networkManagerPrefab;
        [SerializeField] private SessionInfo _sessionInfoPrefab;

        [Header("Domain configs")]
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private UIConfig _uiConfig;
        [SerializeField] private ConnectionConfig _connectionConfig;
        
        public int MaxPlayers => _maxPlayers;
        
        public UIConfig UIConfig => _uiConfig;
        public PlayerConfig PlayerConfig => _playerConfig;

        public ConnectionConfig ConnectionConfig => _connectionConfig;
        public SessionInfo SessionInfoPrefab => _sessionInfoPrefab;
        public NetworkManager NetworkManagerPrefab => _networkManagerPrefab;
    }
}
