using System.Threading;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Configs;

namespace Scripts.Runtime.Core.Modules
{
    public class ConnectionConfigModule : IBootstrapModule
    {
        private readonly GameConfig _gameConfig;

        public ConnectionConfigModule(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        public async UniTask InitializeAsync(CancellationToken ct)
        {
            _gameConfig.ConnectionConfig.TryLoadSettings(); // it can be made with async if config is too big
            await UniTask.Yield();
        }
    }
}
