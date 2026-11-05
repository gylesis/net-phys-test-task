using System;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Runtime.Core
{
    public class SessionInfo : NetworkBehaviour
    {
        public static SessionInfo Instance { get; private set; }
        
        public const float PingReportInterval = 1f;
        public bool AmIRegisteredMyself { get; set; }
        public NetworkList<PlayerInfo> PlayersInfo = new();

        private float _nextPingReport;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Instance = this;
        }

        private void Update()
        {
            ReportPing();
        }

        private void ReportPing()
        {
            if (!IsSpawned || !IsClient || IsServer)
                return;
            
            _nextPingReport -= Time.unscaledDeltaTime;
            if (_nextPingReport > 0f)
                return;
            _nextPingReport = PingReportInterval;

            var transport = NetworkManager.NetworkConfig.NetworkTransport;
            ulong rtt = transport.GetCurrentRtt(NetworkManager.ServerClientId);
            ReportPingServerRpc((ushort)Math.Min(rtt, ushort.MaxValue));
        }

        public bool IsSteamUserAlreadyExist(ulong steamId)
        {
            foreach (var playerInfo in PlayersInfo)
            {
                if (playerInfo.SteamID == steamId)
                    return true;
            }
            return false;
        }
        
        [Rpc(SendTo.Server)]
        private void ReportPingServerRpc(ushort pingMs, RpcParams rpcParams = default)
        {
            var clientId = rpcParams.Receive.SenderClientId;

            for (var index = 0; index < PlayersInfo.Count; index++)
            {
                var playerInfo = PlayersInfo[index];
                if (playerInfo.ClientId == clientId)
                {
                    playerInfo.LastPing = pingMs;
                    
                    PlayersInfo.Set(index, playerInfo);
                    return;
                }
            }
        }

        [Rpc(SendTo.Server)]
        public void RegisterMyselftDataRpc(ulong steamId, string name, RpcParams rpcParams = default)
        {
            var playerInfo = new PlayerInfo();

            playerInfo.ClientId = rpcParams.Receive.SenderClientId;
            playerInfo.SteamID = steamId;
            playerInfo.Name = name;
            
            PlayersInfo.Add(playerInfo);
        }

        public void HandleClientDisconnect(ulong clientId)
        {
            PlayerInfo targetPlayer = default;
            
            foreach (var playerInfo in PlayersInfo)
            {
                if (playerInfo.ClientId == clientId)
                {
                    targetPlayer = playerInfo;
                }
            }

            if (targetPlayer.ClientId != 0) 
                PlayersInfo.Remove(targetPlayer);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Instance = null;
        }
    }
}