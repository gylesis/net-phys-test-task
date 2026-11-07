using Scripts.Runtime.Configs;
using Scripts.Runtime.Core;
using Scripts.Runtime.Core.Modules;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.PhysItems;
using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.Core.States;
using Scripts.Runtime.Services;
using Scripts.Runtime.UI;
using Scripts.Runtime.Utils;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Runtime.Installers
{
    public class RootScope : LifetimeScope
    {
        public static RootScope Instance { get; private set; }

        public IDIResolver IdiResolver => Container != null ? Container.Resolve<IDIResolver>() : null;

        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private Curtains _curtains;
        [SerializeField] private ConnectionManager _connectionManager;
        [SerializeField] private UIController _uiController;

        protected override void Awake()
        {
            Instance = this;
            base.Awake();
        }

        protected override void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            base.OnDestroy();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameConfig);
            builder.RegisterInstance(_curtains);

            builder.RegisterComponent(_connectionManager);
            builder.RegisterComponent(_uiController);

            builder.Register<ModulesFacade>(Lifetime.Singleton);

            builder.Register<InputService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.Register<IDIResolver, VContainerDiResolver>(Lifetime.Singleton);

            builder.Register<IBootstrapModule, ConnectionConfigModule>(Lifetime.Singleton).AsSelf();
            builder.Register<IBootstrapModule, InputModule>(Lifetime.Singleton).AsSelf();
            builder.Register<IBootstrapModule, SteamModule>(Lifetime.Singleton).AsSelf();

            builder.Register<GameStateMachine>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.Register<LocalPlayerContext>(Lifetime.Singleton);
            builder.Register<GameplaySceneContext>(Lifetime.Singleton);
            builder.Register<InteractionSystem>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<PlayerSpawner>(Lifetime.Singleton).AsSelf();
            builder.Register<ItemSpawner>(Lifetime.Singleton).AsSelf();

            builder.RegisterInstance(_sceneLoader);

            builder.Register<GameState, BootstrapState>(Lifetime.Singleton).AsSelf();
            builder.Register<GameState, LoadMainMenuState>(Lifetime.Singleton).AsSelf();
            builder.Register<GameState, MainMenuState>(Lifetime.Singleton).AsSelf();
            builder.Register<GameState, LoadGameplayState>(Lifetime.Singleton).AsSelf();
            builder.Register<GameState, GameplayState>(Lifetime.Singleton).AsSelf();
        }
    }
}
