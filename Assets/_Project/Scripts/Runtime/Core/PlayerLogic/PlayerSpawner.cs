using Scripts.Runtime.Configs;
using Scripts.Runtime.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Runtime.Core.PlayerLogic
{
    public class PlayerSpawner
    {
        private readonly GameplaySceneRefs _gameplaySceneRefs;
        private readonly GameConfig _gameConfig;

        public PlayerSpawner(GameplaySceneRefs gameplaySceneRefs, GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
            _gameplaySceneRefs = gameplaySceneRefs;
        }

        public PlayerController SpawnPlayer(ulong playerId)
        {
            var playerPrefab = _gameConfig.PlayerConfig.PlayerPrefab;
            var position = _gameplaySceneRefs.SpawnPoints.GetRandomItem().position;

            var playerInstance = Object.Instantiate(playerPrefab, position, Quaternion.identity);
            playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);

            return playerInstance;
        }
    }
}