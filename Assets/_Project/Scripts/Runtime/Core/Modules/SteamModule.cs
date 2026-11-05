using System.Threading;
using Cysharp.Threading.Tasks;
using Steamworks;
using Constants = Scripts.Runtime.Utils.Constants;

namespace Scripts.Runtime.Core.Modules
{
    public class SteamModule : IBootstrapModule
    {
        public int Order { get; }
        
        public bool IsInitialized { get; private set; }
        
        public ulong SteamId { get; private set; }
        
        public async UniTask InitializeAsync(CancellationToken ct)
        {
            //if (!SteamManager.Initialized)
            //    return;

            SteamId = Constants.SteamIdNotAvailable;
            
            await UniTask.Yield();

            if (IsInitialized)
            {
                SteamId = SteamUser.GetSteamID().m_SteamID;
            }
        }

        public string GetMyName() => SteamFriends.GetPersonaName();
        public string GetSteamName(ulong steamId)
        {
            return SteamFriends.GetFriendPersonaName(new CSteamID(steamId));
        }
    }
}