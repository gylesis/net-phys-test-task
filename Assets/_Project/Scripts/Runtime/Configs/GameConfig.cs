using Unity.Netcode;
using UnityEngine;

namespace Scripts.Runtime.Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "NetPhys/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Session")]
        [Min(2)] [SerializeField] private int _maxPlayers = 4;
        [SerializeField] private NetworkManager _networkManagerPrefab;
        
        [Header("Domain configs")]
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private UIConfig _uiConfig;

        public int MaxPlayers => _maxPlayers;

        public UIConfig UIConfig => _uiConfig;
        public PlayerConfig PlayerConfig => _playerConfig;

        public NetworkManager NetworkManagerPrefab => _networkManagerPrefab;
    }
}
