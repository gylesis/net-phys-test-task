using System.Threading;
using Cysharp.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using Constants = Scripts.Runtime.Utils.Constants;

namespace Scripts.Runtime.Core.Modules
{
    public class SteamModule : IBootstrapModule
    {
        private const uint FallbackAppId = 480;
        private const string AppIdFileName = "steam_appid.txt";

        private static uint ResolveAppId()
        {
            try
            {
                string path = System.IO.Path.Combine(Application.dataPath, "..", AppIdFileName);
                string content = System.IO.File.ReadAllText(path).Trim();
                return uint.Parse(content);
            }
            catch (System.Exception)
            {
                return FallbackAppId;
            }
        }

        public int Order { get; }

        public bool IsInitialized { get; private set; }

        public ulong SteamId { get; private set; }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            IsInitialized = false;
            SteamId = Constants.SteamIdNotAvailable;

            try
            {
                SteamClient.Init(ResolveAppId());
            }
            catch (System.Exception)
            {
                return;
            }

            IsInitialized = true;
            SteamId = SteamClient.SteamId.Value;

            PumpCallbacks(ct).Forget();

            await UniTask.Yield();
        }

        private async UniTaskVoid PumpCallbacks(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                SteamClient.RunCallbacks();
                await UniTask.Yield();
            }

            SteamClient.Shutdown();
            IsInitialized = false;
        }

        public string GetMyName() => IsInitialized ? SteamClient.Name : "";

        public async UniTask<string> GetSteamNameAsync(ulong steamId, CancellationToken ct = default)
        {
            if (!IsInitialized)
                return "";

            var id = (SteamId)steamId;
            float timeoutTime = Time.realtimeSinceStartup + 5f;

            while (SteamFriends.RequestUserInformation(id, false) && Time.realtimeSinceStartup < timeoutTime)
            {
                await UniTask.Delay(50, DelayType.Realtime, cancellationToken: ct);
            }

            return new Friend(id).Name;
        }
    }
}
