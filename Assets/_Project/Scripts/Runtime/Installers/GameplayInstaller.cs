using Scripts.Runtime.Core;
using Scripts.Runtime.Core.CameraLogic;
using Scripts.Runtime.Core.PlayerLogic;
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
            builder.RegisterInstance(_gameplaySceneRefs);

            builder.RegisterComponentInHierarchy<CameraController>();

            builder.RegisterBuildCallback(container =>
            {
                var camera = container.Resolve<CameraController>();
                container.Resolve<GameplaySceneContext>().Setup(_gameplaySceneRefs, camera);
            });
        }
    }
}
