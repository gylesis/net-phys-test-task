using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scripts.Runtime.Core.Modules
{
    public interface IBootstrapModule
    {
        UniTask InitializeAsync(CancellationToken ct);
    }
}
