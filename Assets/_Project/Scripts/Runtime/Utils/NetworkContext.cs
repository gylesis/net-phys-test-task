using Scripts.Runtime.Installers;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Runtime.Utils
{
    public class NetworkContext : NetworkBehaviour
    {
        private void Awake()
        {
            EnsureInjected();
        }

        protected void EnsureInjected()
        {
            var resolver = RootScope.Instance != null ? RootScope.Instance.IdiResolver : null;

            if (resolver == null)
            {
                Debug.LogError($"{nameof(NetworkContext)}: the root container is not built; dependencies will stay unresolved.", this);
                return;
            }

            resolver.Inject(gameObject);
        }
    }
}
