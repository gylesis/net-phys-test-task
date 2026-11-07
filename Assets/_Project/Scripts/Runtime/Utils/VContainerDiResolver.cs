using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Runtime.Utils
{
    public class VContainerDiResolver : IDIResolver
    {
        private readonly IObjectResolver _resolver;

        public VContainerDiResolver(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        public T Resolve<T>() where T : class => _resolver.Resolve<T>();

        public T Instantiate<T>(T prefab) where T : Component => _resolver.Instantiate(prefab);

        public T Instantiate<T>(T prefab, Transform parent) where T : Component => _resolver.Instantiate(prefab, parent);

        public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component =>
            _resolver.Instantiate(prefab, position, rotation);

        public void Inject(GameObject gameObject) => _resolver.InjectGameObject(gameObject);
    }
}
