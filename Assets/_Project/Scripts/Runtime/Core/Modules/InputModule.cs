using System.Threading;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Services;

namespace Scripts.Runtime.Core.Modules
{
    public class InputModule : IBootstrapModule
    {
        private readonly InputService _input;

        public InputModule(InputService input)
        {
            _input = input;
        }

        public UniTask InitializeAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _input.Initialize();
            return UniTask.CompletedTask;
        }
    }
}
