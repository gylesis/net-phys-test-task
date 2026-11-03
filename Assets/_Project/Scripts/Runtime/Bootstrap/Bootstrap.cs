using Scripts.Runtime.Core.States;
using Scripts.Runtime.Utils;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.Bootstrap
{
    public class Bootstrap : MonoBehaviour
    {
        private GameStateMachine _gameStateMachine;

        [Inject]
        private void Construct(GameStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        private void Start()
        {
            _gameStateMachine = DiInjecter.Instance.Resolve<GameStateMachine>();
            _gameStateMachine.Enter<BootstrapState>();
        }
    }
}
