using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Runtime.Core
{
    public class GameplaySceneRefs : MonoBehaviour
    {
        [SerializeField] private List<Transform> _spawnPoints;

        public List<Transform> SpawnPoints => _spawnPoints;
    }
}