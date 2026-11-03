using System;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Utils;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Runtime.Services
{
    /// <summary>
    /// Central input access point. Resolves actions through InputActionReference
    /// fields from PlayerConfig and exposes frame-friendly properties, so gameplay
    /// code never touches InputAction objects directly. Actions are enabled on
    /// initialization and disabled on dispose.
    /// </summary>
    public class InputService : IDisposable, ITickable
    {
        private readonly PlayerConfig _config;
        
        private BetterActionReference _move;
        private BetterActionReference _look;
        private BetterActionReference _jump;
        private BetterActionReference _sprint;

        public bool IsEnabled { get; private set; }

        public Vector2 Move { get; private set; }

        public Vector2 Look { get; private set; }

        public bool JumpPressed { get; private set; }

        public bool SprintPressed { get; private set; }

        [Inject]
        public InputService(GameConfig gameConfig)
        {
            _config = gameConfig.PlayerConfig;
        }

        /// <summary>Resolves references and enables input. Called once from the bootstrap.</summary>
        public void Initialize()
        {
            _move = _config.Move;
            _look = _config.Look;
            _jump = _config.Jump;
            _sprint = _config.Sprint;

            IsEnabled = true;
        }

        /// <summary>Reads current frame values. Call once per frame from a runner.</summary>
        public void Tick()
        {
            if (!IsEnabled)
                return;
            
            Move = _move.ReadValue<Vector2>();
            Look = _look.ReadValue<Vector2>();
            JumpPressed = _jump.IsPerformed();
            SprintPressed = _sprint.IsPerformed();
        }

        public void Dispose()
        {
            IsEnabled = false;
        }

    }
}
