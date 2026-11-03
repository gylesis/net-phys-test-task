using System;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Scripts.Runtime.Utils
{
    public class DiInjecter : IInitializable, IDisposable
    {
        public static DiInjecter Instance { get; private set; }

        // Без Domain Reload (Enter Play Mode Settings → Reload Scene only) статика переживает выход
        // из плеймода: ссылка осталась бы указывать на объект ПРОШЛОЙ сессии — с мёртвым скоупом
        // внутри. Это хуже, чем null: обращение не падает, а тихо работает не с тем объектом.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;
        private LifetimeScope _installer;
        
        public DiInjecter(LifetimeScope scope)
        {
            Instance = this;
            
            SetupScope(scope);
            //MyNetworkRoomManager.singleton.OnClientSceneChangedAction.Subscribe(unit => OnSceneLoaded());
        }
        
        public void SetupScope(LifetimeScope scope)
        {
            _installer = scope;
        }

        public void Inject(MonoBehaviour monoBehaviour)
        {
            Inject(monoBehaviour.gameObject);
        }

        public void SetParent(LifetimeScope lifetimeScope, GameObject gameObject)
        {
            lifetimeScope.parentReference = new ParentReference()
            {
                Object = _installer,
                TypeName = _installer.GetType().FullName
            };
            lifetimeScope.Build();
            
            lifetimeScope.Container.InjectGameObject(gameObject);
        }
        
        public LifetimeScope CurrentScope
        {
            get
            {
                EnsureLiveScope();
                return _installer;
            }
        }

        public void Inject(GameObject gameObject)
        {
            EnsureLiveScope();
            _installer.Container.InjectGameObject(gameObject);
        }

        public T Resolve<T>()
        {
            EnsureLiveScope();

            if (!_installer.Container.TryResolve(typeof(T), out object result))
            {
                foreach (LifetimeScope scope in Object.FindObjectsByType<LifetimeScope>(FindObjectsSortMode.None))
                {
                    if (scope.Container != null && scope.Container.TryResolve(typeof(T), out result))
                    {
                        _installer = scope;
                        return (T)result;
                    }
                }
            }

            return (T)result;
        }

        // Скоуп мог быть уничтожен (рестарт уровня) или ещё не построен (объект проснулся раньше
        // инсталлера сцены) — тогда Container == null. Ищем живой не-корневой скоуп со своим контейнером.
        private void EnsureLiveScope()
        {
            if (_installer != null && _installer.Container != null)
                return;

            foreach (LifetimeScope scope in Object.FindObjectsByType<LifetimeScope>(FindObjectsSortMode.None))
            {
                if (scope.Container == null)
                    continue;

                _installer = scope;
                return;
            }
        }

        public void Initialize()
        {
            // Just for skip Lazy Container Resolve
        }

        public void Dispose()
        {
            _installer.DisposeCore();
        }
    }
}