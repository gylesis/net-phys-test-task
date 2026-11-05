using System.Collections.Generic;
using Scripts.Runtime.UI;
using UnityEngine;

namespace Scripts.Runtime.Configs
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "NetPhys/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [SerializeField] private List<Menu> _menusPrefabs;
        [SerializeField] private List<PopUp> _popUpsPrefabs;
        [SerializeField] private SessionPlayerView _sessionPlayerViewPrefab;

        public SessionPlayerView SessionPlayerViewPrefab => _sessionPlayerViewPrefab;
        public List<PopUp> PopUpsPrefabs => _popUpsPrefabs;
        public List<Menu> MenusPrefabs => _menusPrefabs;
    }
}