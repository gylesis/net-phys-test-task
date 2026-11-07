using Scripts.Runtime.Configs;
using Scripts.Runtime.Utils;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.Core.PlayerLogic
{
    public class PlayerSpawner
    {
        private readonly GameConfig _gameConfig;
        private readonly GameplaySceneContext _sceneContext;
        private readonly IDIResolver _idiResolver;

        public PlayerSpawner(GameConfig gameConfig, GameplaySceneContext sceneContext, IDIResolver idiResolver)
        {
            _gameConfig = gameConfig;
            _sceneContext = sceneContext;
            _idiResolver = idiResolver;
        }

        public PlayerController SpawnPlayer(ulong playerId)
        {
            var playerPrefab = _gameConfig.PlayerConfig.PlayerPrefab;
            var position = _sceneContext.SceneRefs.SpawnPoints.GetRandomItem().position;

            var playerInstance = _idiResolver.Instantiate(playerPrefab, position, Quaternion.identity);
            playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);

            return playerInstance;
        }
    }
}
