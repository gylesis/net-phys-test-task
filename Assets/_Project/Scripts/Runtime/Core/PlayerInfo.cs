using System;
using Unity.Collections;
using Unity.Netcode;

namespace Scripts.Runtime.Core
{
    public struct PlayerInfo : IEquatable<PlayerInfo>, INetworkSerializeByMemcpy
    {
        public ulong ClientId;
        public ulong SteamID;
        public FixedString64Bytes Name;
        public ushort LastPing;

        public bool Equals(PlayerInfo other)
        {
            return ClientId == other.ClientId && SteamID == other.SteamID && Name.Equals(other.Name) && LastPing == other.LastPing;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, SteamID, Name, LastPing);
        }
    }
    
}