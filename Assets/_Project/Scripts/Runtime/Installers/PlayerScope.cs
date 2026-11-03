using Scripts.Runtime.Core.PlayerLogic;
using VContainer;
using VContainer.Unity;

namespace Scripts.Runtime.Installers
{
    public class PlayerScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<InteractionSystem>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }
    }
}