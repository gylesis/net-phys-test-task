using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scripts.Runtime.Core.Modules
{
    public interface IBootstrapModule
    {
        int Order { get; }

        UniTask InitializeAsync(CancellationToken ct);
    }
}
