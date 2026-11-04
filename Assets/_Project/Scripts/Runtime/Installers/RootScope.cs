using Scripts.Runtime.Bootstrap;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core;
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
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private Curtains _curtains;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameConfig);
            builder.RegisterInstance(_curtains);

            builder.Register<InputService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<DiInjecter>(Lifetime.Singleton).WithParameter(this).AsImplementedInterfaces().AsSelf();

            builder.Register<IBootstrapModule, InputModule>(Lifetime.Singleton).AsSelf();
            builder.Register<IBootstrapModule, SteamModule>(Lifetime.Singleton).AsSelf();
            
            builder.Register<GameStateMachine>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.RegisterInstance(_sceneLoader);
            
            builder.Register<GameState, BootstrapState>(Lifetime.Singleton).AsSelf();
            builder.Register<GameState, LoadMainMenuState>(Lifetime.Singleton).AsSelf();
            builder.Register<GameState, MainMenuState>(Lifetime.Singleton).AsSelf();
            builder.Register<GameState, LoadGameplayState>(Lifetime.Singleton).AsSelf();
            builder.Register<GameState, GameplayState>(Lifetime.Singleton).AsSelf();
        }
    }
}
