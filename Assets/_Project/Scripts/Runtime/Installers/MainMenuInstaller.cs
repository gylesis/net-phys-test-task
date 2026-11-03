using Scripts.Runtime.Core;
using Scripts.Runtime.Utils;
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
            DiInjecter.Instance.SetupScope(this);
            builder.RegisterInstance<MainMenuNetController>(_menuNetController);
        }
    }
}