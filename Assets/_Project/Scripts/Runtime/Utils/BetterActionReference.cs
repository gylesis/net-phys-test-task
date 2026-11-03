using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts.Runtime.Utils
{
    [Serializable]
    public struct BetterActionReference
    {
        [SerializeField] private InputActionReference _reference;

        public InputAction Action
        {
            get
            {
                if (_reference == null)
                    return null;
                return _reference.action;
            }
        }

        public bool TryGetAction(out InputAction action)
        {
            action = Action;
            return action != null;
        }

        /// <summary>
        /// Modifiers is accounted when reading
        /// </summary>
        public TValue ReadValue<TValue>() where TValue : struct
        {
            var value = default(TValue);

            var action = Action;
            if (action != null)
                value = action.ReadValue<TValue>();

            return value;
        }

        public bool IsPerformed()
        {
            var value = false;

            var action = Action;
            if (action != null)
                value = action.IsPerformed();

            return value;
        }
        
        /// <summary>Кнопка зажата (см. <see cref="BetterActionReferenceExtensions.IsHeld"/>).</summary>
        public bool IsHeld()
        {
            var action = Action;

            return action != null && action.IsHeld();
        }

        public bool IsPressedOnce()
        {
            var value = false;

            var action = Action;
            if (action != null)
                value = action.WasPressedThisFrame();

            return value;
        }
        
        public bool IsReleased()
        {
            var value = false;

            var action = Action;
            if (action != null)
                value = action.WasReleasedThisFrame();

            return value;
        }

    }
    
    public static class BetterActionReferenceExtensions
    {
        /// <summary>
        /// This sligtly worse than action with CallbackContext
        /// </summary>
        public static void SubscribeToPerformed(this InputAction inputAction, Action action) => SubscribeToPerformed(inputAction, c => action());

        public static void SubscribeToPerformed(this InputAction inputAction, Action<InputAction.CallbackContext> action) => SubscribeToPhase(inputAction, InputActionPhase.Performed, action);

        /// <summary>
        /// This sligtly worse than action with CallbackContext
        /// </summary>
        public static void SubscribeToPhase(this InputAction inputAction, InputActionPhase phase, Action action) => SubscribeToPhase(inputAction, phase, c => action());

        public static void SubscribeToPhase(this InputAction inputAction, InputActionPhase phase, Action<InputAction.CallbackContext> action)
        {
            if (inputAction == null)
                return;
            switch (phase)
            {
                case InputActionPhase.Started: inputAction.started += action; break;
                case InputActionPhase.Performed: inputAction.performed += action; break;
                case InputActionPhase.Canceled: inputAction.canceled += action; break;
                default: throw new NotSupportedException($"Phase {phase} not supported");
            }
        }

        public static bool IsPerformed(this InputAction action)
        {
            return action != null && action.phase == InputActionPhase.Performed;
        }
        
        public static bool IsPressed(this InputAction action)
        {
            return action != null && action.phase == InputActionPhase.Started;
        }

        /// <summary>
        /// Кнопка ЗАЖАТА прямо сейчас.
        ///
        /// Не через phase: у Button-действия с обычной интеракцией фаза Performed держится один кадр
        /// (нажатие), а Started — до отпускания только у части конфигураций. Значение же читается
        /// одинаково при любой настройке действия, поэтому «зажато» надёжнее спрашивать им.
        /// </summary>
        public static bool IsHeld(this InputAction action)
        {
            return action != null && action.ReadValue<float>() > 0.5f;
        }

        public static bool IsPerformed(this InputAction action, bool valueWhenNull)
        {
            return action != null ? action.phase == InputActionPhase.Performed : valueWhenNull;
        }
    }
}