using Scripts.Runtime.Core;
using Scripts.Runtime.Core.CameraLogic;
using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.Utils;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Runtime.Installers
{
    public class GameplayInstaller : LifetimeScope
    {
        [SerializeField] private GameplaySceneRefs _gameplaySceneRefs;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<GameplaySceneRefs>(_gameplaySceneRefs);
            
            builder.RegisterComponentInHierarchy<CameraController>();
            builder.Register<PlayerSpawner>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            
            DiInjecter.Instance.SetupScope(this);
        }
    }
}
