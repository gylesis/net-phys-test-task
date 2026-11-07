using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Utils;
using UniRx;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Transform _menusParent;
        [SerializeField] private Transform _popupsParent;

        private Dictionary<Type, Menu> _activeMenus = new Dictionary<Type, Menu>();
        private Dictionary<Type, Menu> _menusPrefabs;
        private InformPopUp _informPopUp;
        private Action _closeInformPopUpCallback;

        private GameConfig _gameConfig;
        private IDIResolver _diResolver;

        [Inject]
        private void Construct(GameConfig gameConfig, IDIResolver idiResolver)
        {
            _diResolver = idiResolver;
            _gameConfig = gameConfig;
        }

        private void Start()
        {
            _menusPrefabs = _gameConfig.UIConfig.MenusPrefabs.ToDictionary(x => x.GetType(), y => y);
        }

        public void ShowInfoPopUp(string title, string description, Action onClose = null)
        {
            _closeInformPopUpCallback = onClose;
            
            if (_informPopUp == null)
            {
                var informPopUpPrefab = _gameConfig.UIConfig.PopUpsPrefabs.FirstOrDefault(x => x.GetType() == typeof(InformPopUp));
                
                if (informPopUpPrefab == null)
                {
                    throw new NullReferenceException("InformPopUp prefab not found");
                }

                _informPopUp = _diResolver.Instantiate(informPopUpPrefab, _popupsParent) as InformPopUp;
                _informPopUp.Closed.Subscribe(unit => HandleInformPopUpClosed()).AddTo(destroyCancellationToken);
            }
            
            _informPopUp.SetDescription(title, description);
            _informPopUp.Show();
        }

        private void HandleInformPopUpClosed()
        {
            _closeInformPopUpCallback?.Invoke();
            _informPopUp.Hide();
        }

        public TMenuType ShowMenu<TMenuType>() where TMenuType : Menu
        {
            var menuType = typeof(TMenuType);

            if (!_activeMenus.TryGetValue(menuType, out var menu)) 
                menu = SpawnMenu<TMenuType>();
            
            menu.Show();
            
            return menu as TMenuType;
        }

        public void HideAllActiveMenus()
        {
            foreach (var pair in _activeMenus) 
                pair.Value.Hide();
        }
        
        public void HideMenu<TMenuType>() where TMenuType : Menu
        {
            var menuType = typeof(TMenuType);

            if (!_activeMenus.TryGetValue(menuType, out var menu)) 
                return;
            
            menu.Hide();
        }

        public void FlushGameplayMenus()
        {
            List<Type> toRemove = new List<Type>();

            foreach (var pair in _activeMenus)
            {
                if (IsGameplayMenu(pair.Key))
                    toRemove.Add(pair.Key);
            }

            foreach (var menuType in toRemove)
            {
                if (!_activeMenus.TryGetValue(menuType, out var menu))
                    continue;

                _activeMenus.Remove(menuType);
                menu.Hide();
                Destroy(menu.gameObject);
            }

            bool IsGameplayMenu(Type menuType)
            {
                return menuType == typeof(GameplayPauseMenu) || menuType == typeof(GameplayHUDMenu);
            }
        }

        private TMenuType SpawnMenu<TMenuType>() where TMenuType : Menu
        {
            var menuType = typeof(TMenuType);

            if (!_menusPrefabs.TryGetValue(menuType, out var menuPrefab))
                throw new KeyNotFoundException($"No menu prefab found for {menuType}. Ensure you added menu in {nameof(UIConfig)}");

            var menuInstance = _diResolver.Instantiate(menuPrefab, _menusParent);
            _activeMenus.TryAdd(menuType, menuInstance);

            return menuInstance as TMenuType;
        }
        
    }
}