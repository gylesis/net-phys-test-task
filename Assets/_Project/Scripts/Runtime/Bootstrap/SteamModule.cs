using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scripts.Runtime.Bootstrap
{
    public class SteamModule : IBootstrapModule
    {
        public int Order { get; }
        public async UniTask InitializeAsync(CancellationToken ct)
        {
            //if (!SteamManager.Initialized)
            //    return;
            
            await UniTask.Yield();
        }
    }
}