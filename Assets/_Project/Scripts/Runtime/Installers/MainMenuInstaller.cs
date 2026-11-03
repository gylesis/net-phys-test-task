using Scripts.Runtime.Utils;
using VContainer;
using VContainer.Unity;

namespace Scripts.Runtime.Installers
{
    public class MainMenuInstaller : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            DiInjecter.Instance.SetupScope(this);
        }
    }
}