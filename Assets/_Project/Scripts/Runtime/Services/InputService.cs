using System;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Utils;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Runtime.Services
{
    public class InputService : IDisposable, ITickable
    {
        private readonly PlayerConfig _config;

        private BetterActionReference MoveAction => _config.Move;
        private BetterActionReference LookAction => _config.Look;
        private BetterActionReference JumpAction => _config.Jump;
        private BetterActionReference SprintAction => _config.Sprint;
        private BetterActionReference InteractAction => _config.Interact;
        private BetterActionReference ThrowAction => _config.Throw;
        private BetterActionReference DeleteItemAction => _config.DeleteItem;
        private BetterActionReference PauseAction => _config.Pause;

        public bool IsEnabled { get; private set; }

        public Vector2 Move { get; private set; }

        public Vector2 Look { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool SprintPressed { get; private set; }
        
        public bool InteractPressedOnce { get; private set; }
        public bool ThrowPressedOnce { get; private set; }
        public bool DeletePressedOnce { get; private set; }
        public bool PausePressedOnce { get; private set; }
        public bool Interact { get; private set;}

        [Inject]
        public InputService(GameConfig gameConfig)
        {
            _config = gameConfig.PlayerConfig;
        }

        public void Initialize()
        {
            IsEnabled = true;
        }

        public void Tick()
        {
            if (!IsEnabled)
                return;
            
            Move = MoveAction.ReadValue<Vector2>();
            Look = LookAction.ReadValue<Vector2>();
            JumpPressed = JumpAction.IsPerformed();
            SprintPressed = SprintAction.IsPerformed();
            Interact = InteractAction.IsPerformed();
            InteractPressedOnce = InteractAction.IsPressedOnce();
            ThrowPressedOnce = ThrowAction.IsPressedOnce();
            DeletePressedOnce = DeleteItemAction.IsPressedOnce();
            PausePressedOnce = PauseAction.IsPressedOnce();
        }

        public void Dispose()
        {
            IsEnabled = false;
        }

    }
}
