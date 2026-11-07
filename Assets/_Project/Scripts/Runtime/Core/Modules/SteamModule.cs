using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using Steamworks;
using Steamworks.Data;
using UniRx;
using UnityEngine;
using Constants = Scripts.Runtime.Utils.Constants;

namespace Scripts.Runtime.Core.Modules
{
    public class SteamModule : IBootstrapModule
    {
        private const uint FallbackAppId = 480;
        private const string AppIdFileName = "steam_appid.txt";
        
        public bool IsInitialized { get; private set; }

        public ulong SteamId { get; private set; }

        public Subject<string> GameJoinRequested { get; } = new Subject<string>();
        public Subject<ulong> LobbyJoinRequested { get; } = new Subject<ulong>();
        public Subject<Unit> LobbyOwnerLeft { get; } = new Subject<Unit>();

        public Lobby? CurrentLobby { get; private set; }

        private readonly Dictionary<ulong, string> _resolvedNames = new Dictionary<ulong, string>();

        public ulong CurrentLobbyOwnerId => CurrentLobby.HasValue && CurrentLobby.Value.Owner.Id.IsValid
            ? CurrentLobby.Value.Owner.Id.Value
            : Constants.SteamIdNotAvailable;

        private readonly GameConfig _gameConfig;
        
        public SteamModule(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }
        
        public async UniTask InitializeAsync(CancellationToken ct)
        {
            IsInitialized = false;
            SteamId = Constants.SteamIdNotAvailable;

            if (!_gameConfig.ConnectionConfig.Settings.IsSteamTransport)
                return;
            
            try
            {
                SteamClient.Init(ResolveAppId());
            }
            catch (System.Exception)
            {
                _gameConfig.ConnectionConfig.Settings.IsSteamTransport = false;
                Debug.LogError($"Steam is not configured");
                return;
            }

            IsInitialized = true;
            SteamId = SteamClient.SteamId.Value;

            SteamFriends.OnGameRichPresenceJoinRequested += HandleGameRichPresenceJoinRequested;
            SteamFriends.OnGameLobbyJoinRequested += HandleGameLobbyJoinRequested;
            SteamMatchmaking.OnLobbyMemberLeave += HandleLobbyMemberLeave;

            PumpCallbacks(ct).Forget();

            await UniTask.Yield();
        }
        
        private uint ResolveAppId()
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

        private async UniTaskVoid PumpCallbacks(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                SteamClient.RunCallbacks();
                await UniTask.Yield();
            }

            SteamFriends.OnGameRichPresenceJoinRequested -= HandleGameRichPresenceJoinRequested;
            SteamFriends.OnGameLobbyJoinRequested -= HandleGameLobbyJoinRequested;
            SteamMatchmaking.OnLobbyMemberLeave -= HandleLobbyMemberLeave;
            CurrentLobby = null;
            SteamClient.Shutdown();
            IsInitialized = false;
        }

        private void HandleGameRichPresenceJoinRequested(Friend friend, string connectString)
        {
            GameJoinRequested.OnNext(connectString);
        }

        private void HandleGameLobbyJoinRequested(Lobby lobby, SteamId friendId)
        {
            LobbyJoinRequested.OnNext(lobby.Id.Value);
        }

        private void HandleLobbyMemberLeave(Lobby lobby, Friend member)
        {
            if (!CurrentLobby.HasValue || lobby.Id != CurrentLobby.Value.Id)
                return;

            if (member.Id.Value == CurrentLobbyOwnerId && member.Id.Value != SteamId)
                LobbyOwnerLeft.OnNext(Unit.Default);
        }

        // FacepunchTransport shuts Steam down on NetworkManager.Shutdown (e.g. when leaving to the main menu),
        // so the client has to be re-initialized before the next lobby operation in the same session.
        private bool EnsureSteamClient()
        {
            if (SteamClient.IsValid)
                return true;

            try
            {
                SteamClient.Init(ResolveAppId());
            }
            catch (System.Exception)
            {
                return false;
            }

            SteamFriends.OnGameRichPresenceJoinRequested += HandleGameRichPresenceJoinRequested;
            SteamFriends.OnGameLobbyJoinRequested += HandleGameLobbyJoinRequested;
            SteamMatchmaking.OnLobbyMemberLeave += HandleLobbyMemberLeave;

            SteamId = SteamClient.SteamId.Value;
            return true;
        }

        public async UniTask<bool> CreateLobbyAsync(string name, int maxPlayers, bool friendsOnly, CancellationToken ct = default)
        {
            if (!IsInitialized || !EnsureSteamClient())
                return false;

            LeaveLobby();

            var lobby = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
            ct.ThrowIfCancellationRequested();

            if (!lobby.HasValue)
                return false;

            var value = lobby.Value;

            if (friendsOnly)
                value.SetFriendsOnly();
            else
                value.SetPrivate();

            value.SetData("name", string.IsNullOrEmpty(name) ? GetMyName() : name);
            value.SetGameServer(SteamId);
            value.SetJoinable(true);

            CurrentLobby = value;
            return true;
        }

        public async UniTask<ulong?> JoinLobbyAsync(ulong lobbyId, CancellationToken ct = default)
        {
            if (!IsInitialized || !EnsureSteamClient())
                return null;

            LeaveLobby();

            var lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
            ct.ThrowIfCancellationRequested();

            if (!lobby.HasValue)
                return null;

            CurrentLobby = lobby.Value;
            var ownerId = CurrentLobbyOwnerId;
            return ownerId != Constants.SteamIdNotAvailable ? ownerId : (ulong?)null;
        }

        public void LeaveLobby()
        {
            if (!CurrentLobby.HasValue)
                return;

            try
            {
                CurrentLobby.Value.Leave();
            }
            catch (System.Exception)
            {
                // ignored
            }

            CurrentLobby = null;
        }

        public void OpenLobbyInviteOverlay()
        {
            if (!IsInitialized || !CurrentLobby.HasValue)
                return;

            SteamFriends.OpenGameInviteOverlay(CurrentLobby.Value.Id);
        }

        public void UpdateLobbyState(int playersCount, int maxPlayers, bool inGameplay)
        {
            if (!IsInitialized || !CurrentLobby.HasValue)
                return;

            var lobby = CurrentLobby.Value;

            lobby.SetData("players", playersCount.ToString());
            lobby.SetJoinable(playersCount < maxPlayers);
        }

        public void SetStatus(string status)
        {
            if (!IsInitialized)
                return;

            SteamFriends.SetRichPresence("status", status);
        }

        public void SetJoinString(string value)
        {
            if (!IsInitialized)
                return;

            SteamFriends.SetRichPresence("connect", value ?? "");
        }

        public void ClearRichPresence()
        {
            if (IsInitialized)
                SteamFriends.ClearRichPresence();
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

        public async UniTask<string> GetSteamNameCachedAsync(ulong steamId, CancellationToken ct = default)
        {
            if (_resolvedNames.TryGetValue(steamId, out string cachedName))
                return cachedName;

            string steamName = await GetSteamNameAsync(steamId, ct);

            if (!string.IsNullOrEmpty(steamName))
                _resolvedNames[steamId] = steamName;

            return steamName;
        }
    }
}
