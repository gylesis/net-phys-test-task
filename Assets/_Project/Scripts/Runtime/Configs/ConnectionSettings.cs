using System;
using UnityEngine;

namespace Scripts.Runtime.Configs
{
    // Example connection_config.json:
    // {
    //     "IsSteamTransport": true
    // }
    [Serializable]
    public class ConnectionSettings
    {
        [Tooltip("Use Steam P2P transport (Steam Datagram Relay) instead of a direct IP connection.")]
        public bool IsSteamTransport;
    }
}
