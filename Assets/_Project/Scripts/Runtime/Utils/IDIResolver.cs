using UnityEngine;

namespace Scripts.Runtime.Utils
{
    public interface IDIResolver
    {
        T Resolve<T>() where T : class;
        T Instantiate<T>(T prefab) where T : Component;
        T Instantiate<T>(T prefab, Transform parent) where T : Component;
        T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component;
        void Inject(GameObject gameObject);
    }
}
