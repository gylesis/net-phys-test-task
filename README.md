# NetPhys — co-op physics prototype

A small 2–4 player co-op prototype: players connect to a host, load into a level together and interact with shared physics items. Built as a test task for ARTDOCK.

## Stack

- Unity 6000.3.16f1
- Netcode for GameObjects (host-client topology)
- Unity Transport (IP) / Facepunch Steamworks + Facepunch Transport (Steam P2P via Steam Datagram Relay)
- VContainer (DI), UniRx, UniTask, New Input System

## How to run

### Build

Build the game via `NetPhys -> Build` in the Unity menu. The output folder is highlighted in Explorer.

There are no command-line arguments. All runtime switches live in `connection_config.json` placed next to the game executable (`IsSteamTransport`, plus connection tweaks). Defaults come from the `ConnectionConfig` asset, the JSON overrides them without rebuilding.

### Play over IP (Unity Transport)

1. Ensure `connection_config.json` has `"IsSteamTransport": false` (default).
2. Host: press **Host** in the main menu.
3. Client: enter the host's **IP:port** and press **Join**. Works both across PCs and between several instances on one PC (use `127.0.0.1` with different ports, or Multiplayer Play Mode).

### Play over Steam (Steam P2P)

1. Steam client must be running (test AppID 480, Spacewar).
2. `steam_appid.txt` with `480` must sit next to the game exe.
3. Set `"IsSteamTransport": true` in `connection_config.json`.
4. Host: press **Host**, optionally set a lobby name and visibility (friends-only / private).
5. Clients join via Steam invite (overlay), "Join Game" from the friends list, or by typing the lobby ID and pressing **Join**.

If Steam is not running or fails to initialize, the game logs an error and falls back to the IP transport.

### Local testing

Use Multiplayer Play Mode (MPPM): one instance hosts, others join by IP — no second PC or Steam accounts needed.

### Controls

| Input | Action |
|---|---|
| WASD / arrows | Move |
| Mouse | Camera |
| Space | Jump |
| Left Shift | Sprint |
| E | Grab / drop item |
| Z | Spawn item |
| Left mouse button | Throw held item |
| Q | Delete held item |
| Esc | Pause (player list, ping, kick, skin) |

Bindings are data-driven from `PlayerConfig` (`InputSystem_Actions` asset).

## Architecture

```
Bootstrap scene
  RootScope (VContainer root, created via VContainerSettings) — container lives here
    ├─ Core/States      GameStateMachine: Bootstrap → MainMenu → LoadGameplay → Gameplay
    ├─ Core/Net         ConnectionManager (transport selection, approval, session events)
    ├─ Core/Modules     IBootstrapModule: ConnectionConfig, Input, Steam
    ├─ Core/PlayerLogic PlayerController, PlayerSpawner, InteractionSystem, LocalPlayerContext
    ├─ Core/PhysItems   PhysItem (NetworkRigidbody + NetworkTransform)
    ├─ Services         InputService
    └─ UI               UIController + menus (spawned from prefabs at runtime)
MainMenu / Gameplay scenes: MainMenuInstaller / GameplayInstaller (child scopes of RootScope)
```

Key points:

- **DI via VContainer.** Game code depends on `IDIResolver` (our adapter), not on the container API; swapping the DI framework means rewriting one adapter class. Scenes are wired through installers, cross-scene objects through `GameplaySceneContext` / `LocalPlayerContext`.
- **Bootstrap modules** (`IBootstrapModule`) initialize sequentially at startup in registration order (config load → input → Steam).
- **NGO-instantiated objects** (players, `SessionInfo` on remote peers) never pass through our spawn pipeline, so `NetworkContext` injects their dependencies in `OnNetworkSpawn` through the root scope.
- **State Machine** (`GameStateMachine`) drives the *application* lifecycle: bootstrap, main menu, scene loading, gameplay. Scene-scoped services are re-resolved on every entry into `GameplayState`.

## Networking decisions

**Topology.** Listen server (host-client). The host is authoritative for the session: it spawns all network objects, validates item requests and owns connection approval.

**Connection approval.** The host checks build version, duplicate player by SteamID and the player limit; refusal reasons are delivered to the client and shown in a popup.

**Scene loading.** The gameplay scene is loaded through NGO's `NetworkSceneManager`. The host tracks per-client load completion; the loading screen drops for everyone at once when all clients finish (`OnLoadEventCompleted`). A client that disconnects mid-load counts as timed out, so the rest never get stuck.

**Characters — owner-authoritative.** Each player simulates its own `CharacterController` and publishes the transform through an owner-authoritative `NetworkTransform` with interpolation; remote copies move smoothly without prediction buffers. Rationale: a co-op game with friends, where responsiveness matters more than anti-cheat, so no server-side movement validation (see Known issues).

**Items.** Items are spawned by the host on the player's request (spawn hotkey → server RPC → `ItemSpawner`), belong to no one and survive their creator's exit. Physics runs on the current owner and is replicated by `NetworkRigidbody`.

**Grab.** The grab request is an RPC to the server, which validates distance and that the item is free before setting `IsHeld` and transferring ownership to the holder. Because validation and state change happen atomically on one peer, two players grabbing the same item simultaneously resolves to exactly one winner — no desync.

**Drag & throw.** While held, an item is dragged with a spring-damper force applied at the grab point (REPO-style), simulated by the owner — zero input lag for the holder. Dropping returns ownership to the host. Throwing applies the velocity locally first, then hands ownership back, so the flight is simulated on one peer at any moment and looks identical for everyone; landing never teleports the item.

**Join/leave.** Late joiners load the gameplay scene on their own (nobody waits), get the player list from replicated state and see current item/holding state via `NetworkVariable`s. When the host leaves (or is kicked by the lobby owner leaving), every client shuts down and returns to the menu with a message. A disconnecting player cannot rely on RPC delivery, so the server releases any item they held directly in the despawn path — the item always drops, even on a hard network kill. Player list shows per-player RTT (polled from the transport once a second); the host can kick.

**Steam.** Lobbies are created with name/max players/friends-or-private visibility, kept in sync with the session (joinable flag, player count), and closed for everyone when the host leaves. Rich Presence is set to "In Lobby" / "In Game N/4" plus the connect string for "Join Game" (see Known issues — it is not visible to friends on AppID 480). Nicknames resolve locally from SteamID with caching in `SteamModule`.

**Stats.** Session statistics (items spawned/grabbed/thrown/deleted) live only on the server: they are incremented in host-validated RPC paths into the server-side `GameStats`, which reports each event to `SteamModule.IncrementStat`. That method is a placeholder for the Steamworks stats API (`AddStat` + `StoreStats`) — real publishing needs a paid AppID, same as Rich Presence.

## Known issues

- No host migration — a host exit ends the session for everyone (out of scope per task).
- Movement is not validated server-side; a modified client can move arbitrarily fast.
- Ping is not displayed when the Steam transport is selected.
- Rich Presence status is not visible to friends: publishing it for AppID 480 requires a paid Steamworks app/subscription.
