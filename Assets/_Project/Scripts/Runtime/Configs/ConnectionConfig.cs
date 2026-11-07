using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Scripts.Runtime.Configs
{
    // Runtime connection config. Defaults are passed on load (from GameConfig); a JSON file placed next to the
    // game executable ("connection_config.json") overrides any field of ConnectionSettings without rebuilding the game.
    [CreateAssetMenu(fileName = "ConnectionConfig", menuName = "NetPhys/Connection Config")]
    public class ConnectionConfig : ScriptableObject
    {
        private const string FileName = "connection_config.json";

        [SerializeField] private bool _disableSteamUserCheck;
        [Header("Networking")]
        [SerializeField] private ConnectionSettings _settings = new ConnectionSettings();
        public bool DisableSteamUserCheck => _disableSteamUserCheck;
        public ConnectionSettings Settings => _settings;

        public void TryLoadSettings()
        {
            TryApplyJsonOverride();
        }
        
        private void TryApplyJsonOverride()
        {
            try
            {
                if (Application.isEditor)
                    return;
                
                string path = Path.Combine(Application.dataPath, "..", FileName);

                if (!File.Exists(path))
                    return;

                string json = File.ReadAllText(path);
                var newConnectionSettings = JsonConvert.DeserializeObject<ConnectionSettings>(json);
                _settings = newConnectionSettings;
                
                Debug.Log($"[ConnectionConfig] Applied overrides from {FileName}:\n{json}");
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[ConnectionConfig] Failed to apply {FileName}: {exception.Message}. Default values are used.");
            }
        }
    }
    

}
