using UnityEngine;
using VContainer.Unity;

namespace Scripts.Runtime.Utils
{
    [DisallowMultipleComponent]
    public class AutoInjecter : MonoBehaviour
    {
        private LifetimeScope _lifetimeScope;

        private void Awake()
        {
            if (_lifetimeScope)
            {
                DiInjecter.Instance.SetParent(_lifetimeScope, gameObject);
            }
            else
            {
                DiInjecter.Instance.Inject(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            if (_lifetimeScope != null)
                _lifetimeScope.Dispose();
        }
    }
}