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
        
        public byte SkinIndex;


        public bool Equals(PlayerInfo other)
        {
            return ClientId == other.ClientId && SteamID == other.SteamID && Name.Equals(other.Name) && SkinIndex == other.SkinIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, SteamID, Name, SkinIndex);
        }
    }
    
}