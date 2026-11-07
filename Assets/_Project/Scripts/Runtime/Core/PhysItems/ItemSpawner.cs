using Scripts.Runtime.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Runtime.Core.PhysItems
{
    public class ItemSpawner
    {
        private readonly IDIResolver _idiResolver;

        public ItemSpawner(IDIResolver idiResolver)
        {
            _idiResolver = idiResolver;
        }

        public PhysItem SpawnItem(PhysItem prefab, Vector3 position, Quaternion rotation)
        {
            var instance = _idiResolver.Instantiate(prefab, position, rotation);
            instance.GetComponent<NetworkObject>().Spawn();
            return instance;
        }
    }
}
