using System;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.Utils;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.Core
{
    public class SessionInfo : NetworkContext
    {
        public const float PingReportInterval = 1f;
        public bool AmIRegisteredMyself { get; set; }
        public NetworkList<PlayerInfo> PlayersInfo = new();
        public NetworkList<ushort> PlayersPing = new();
        public NetworkList<NetworkObjectReference> PlayerControlledObjects = new();

        private ConnectionManager _connectionManager;
        private float _nextPingReport;

        [Inject]
        private void Construct(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _connectionManager.AttachSessionInfo(this);
        }

        private void Update()
        {
            ReportPings();
        }

        private void ReportPings()
        {
            if (!IsSpawned || !IsServer)
                return;
            
            _nextPingReport -= Time.unscaledDeltaTime;
            if (_nextPingReport > 0f)
                return;
            _nextPingReport = PingReportInterval;

            if (PlayersPing.Count != PlayersInfo.Count)
                return;

            var transport = NetworkManager.NetworkConfig.NetworkTransport;
            for (var index = 0; index < PlayersInfo.Count; index++)
            {
                var clientId = PlayersInfo[index].ClientId;
                ushort pingMs = 0;

                if (clientId != NetworkManager.ServerClientId)
                    pingMs = (ushort)Math.Min(transport.GetCurrentRtt(clientId), ushort.MaxValue);

                PlayersPing[index] = pingMs;
            }
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

        public void AddPlayerObject(PlayerController playerController)
        {
            if (!IsServer)
                return;

            PlayerControlledObjects.Add(playerController.NetworkObject);
        }

        public void RemovePlayerObject(ulong clientId)
        {
            if (!IsServer)
                return;

            if (NetworkManager.Singleton == null || NetworkManager.Singleton.ShutdownInProgress)
                return;
            
            for (var i = 0; i < PlayerControlledObjects.Count; i++)
            {
                var playerController = PlayerControlledObjects[i];

                if (playerController.TryGet(out var networkObject) && networkObject.OwnerClientId == clientId)
                {
                    PlayerControlledObjects.RemoveAt(i);
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
            PlayersPing.Add(0);
        }

        [Rpc(SendTo.Server)]
        public void ChangeSkinRpc(SkinType newSkinType, RpcParams rpcParams = default)
        {
            ChangePlayerInfo(rpcParams.Receive.SenderClientId, info =>
            {
                info.SkinIndex = (byte)newSkinType; 
                return info;
            });
        }

        private void ChangePlayerInfo(ulong clientId, Func<PlayerInfo, PlayerInfo> change)
        {
            for (var index = 0; index < PlayersInfo.Count; index++)
            {
                var playerInfo = PlayersInfo[index];
                if (playerInfo.ClientId == clientId)
                {
                    PlayersInfo.Set(index, change(playerInfo));
                    return;
                }
            }
        }
        
        public void HandleClientDisconnect(ulong clientId)
        {
            for (var index = 0; index < PlayersInfo.Count; index++)
            {
                if (PlayersInfo[index].ClientId == clientId)
                {
                    PlayersInfo.RemoveAt(index);

                    if (index < PlayersPing.Count)
                        PlayersPing.RemoveAt(index);

                    return;
                }
            }
        }
    }
}