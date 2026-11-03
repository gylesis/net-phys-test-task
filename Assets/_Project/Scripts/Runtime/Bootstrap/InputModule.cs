using System.Threading;
using Cysharp.Threading.Tasks;
using Scripts.Runtime.Services;

namespace Scripts.Runtime.Bootstrap
{
    public class InputModule : IBootstrapModule
    {
        private readonly InputService _input;

        public int Order => 0;

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
