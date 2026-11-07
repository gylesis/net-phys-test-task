using Scripts.Runtime.Core.States;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.Core
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
            _gameStateMachine.Enter<BootstrapState>();
        }
    }
}
