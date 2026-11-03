using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Runtime.Configs;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Transform _menusParent;
        
        public static UIController Instance { get; private set; }
        
        private Dictionary<Type, Menu> _activeMenus = new Dictionary<Type, Menu>();

        private GameConfig _gameConfig;
        private Dictionary<Type, Menu> _menusPrefabs;

        private void Awake()
        {
            Instance = this;
        }

        [Inject]
        private void Construct(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        private void Start()
        {
            _menusPrefabs = _gameConfig.UIConfig.MenusPrefabs.ToDictionary(x => x.GetType(), y => y);
        }

        public TMenuType ShowMenu<TMenuType>() where TMenuType : Menu
        {
            var menuType = typeof(TMenuType);

            if (!_activeMenus.TryGetValue(menuType, out var menu)) 
                menu = SpawnMenu<TMenuType>();
            
            menu.Show();
            
            return menu as TMenuType;
        }
        
        public void HideMenu<TMenuType>() where TMenuType : Menu
        {
            var menuType = typeof(TMenuType);

            if (!_activeMenus.TryGetValue(menuType, out var menu)) 
                return;
            
            menu.Hide();
        }

        private TMenuType SpawnMenu<TMenuType>() where TMenuType : Menu
        {
            var menuType = typeof(TMenuType);

            if (!_menusPrefabs.TryGetValue(menuType, out var menuPrefab))
                throw new KeyNotFoundException($"No menu prefab found for {menuType}. Ensure you added menu in {nameof(UIConfig)}");

            var menuInstance = Instantiate(menuPrefab, _menusParent) as TMenuType;
            _activeMenus.TryAdd(menuType, menuInstance);
            
            return menuInstance;
        }
        
    }
}