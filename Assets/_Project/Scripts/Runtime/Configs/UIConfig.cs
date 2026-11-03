using System.Collections.Generic;
using Scripts.Runtime.UI;
using UnityEngine;

namespace Scripts.Runtime.Configs
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "NetPhys/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [SerializeField] private List<Menu> _menusPrefabs;

        public List<Menu> MenusPrefabs => _menusPrefabs;
    }
}