using System;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core.CameraLogic;
using Scripts.Runtime.Services;
using Scripts.Runtime.Utils;
using UniRx;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.Core.PlayerLogic
{
    [RequireComponent(typeof(CharacterController), typeof(AutoInjecter))]
    public class PlayerController : NetworkContext
    {
        
        [SerializeField] private CharacterController _characterController;

        private PlayerConfig _config;
        private InputService _input;
        private CameraController _cameraController;
        private float _verticalVelocity;

        [Inject]
        private void Construct(GameConfig config, InputService input, CameraController cameraController)
        {
            _config = config.PlayerConfig;
            _input = input;
            _cameraController = cameraController;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                _cameraController.Attach(transform);
                SetCursorLocked(true);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && _cameraController != null)
            {
                SetCursorLocked(false);
                _cameraController.gameObject.SetActive(false);
            }
            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (!IsOwner || !IsSpawned)
                return;

            UpdateMovement();
        }

        private void UpdateMovement()
        {
            Vector2 input = _input.Move;
            // Camera-relative: the keys mean "forward/strafe as seen by the camera".
            Vector3 planar = Quaternion.Euler(0f, _cameraController.Yaw, 0f) * new Vector3(input.x, 0f, input.y);
            planar = Vector3.ClampMagnitude(planar, 1f);

            float speed = _config.MoveSpeed * (_input.SprintPressed ? _config.SprintMultiplier : 1f);

            if (planar.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(planar);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    10f * Time.deltaTime);
            }

            if (_characterController.isGrounded)
            {
                _verticalVelocity = -_config.GroundStickVelocity; // keep grounded on slopes
                if (_input.JumpPressed)
                    _verticalVelocity = _config.JumpVelocity;
            }
            else
            {
                _verticalVelocity += _config.Gravity * Time.deltaTime;
            }

            Vector3 velocity = planar * speed + Vector3.up * _verticalVelocity;
            _characterController.Move(velocity * Time.deltaTime);
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
