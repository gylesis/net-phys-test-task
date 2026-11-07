using Scripts.Runtime.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Runtime.Installers
{
    public class MainMenuInstaller : LifetimeScope
    {
        [SerializeField] private MainMenuNetController _menuNetController;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_menuNetController);
        }
    }
}
