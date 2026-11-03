using Scripts.Runtime.Configs;
using Scripts.Runtime.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Scripts.Runtime.Core.CameraLogic
{
    public class CameraController : MonoBehaviour
    {
        private PlayerConfig _config;
        private InputService _input;

        private Transform _body;
        private float _yaw;
        private float _pitch;
        private bool _attached;

        public float Yaw => _yaw;

        public bool IsLocked => Cursor.lockState == CursorLockMode.Locked;

        [Inject]
        private void Construct(GameConfig config, InputService input)
        {
            _config = config.PlayerConfig;
            _input = input;
        }

        public void Attach(Transform body)
        {
            _body = body;
            _yaw = body != null ? body.eulerAngles.y : 0f;
            _pitch = 0f;
            _attached = body != null;
        }
        
        private void LateUpdate()
        {
            if (!_attached) 
                return;

            if (!IsLocked) 
                return;

            Vector2 delta = _input.Look;
            _yaw += delta.x * _config.LookSensitivity;
            _pitch = Mathf.Clamp(_pitch - delta.y * _config.LookSensitivity, _config.MinPitch, _config.MaxPitch);

            _body.rotation = Quaternion.Euler(0f, _yaw, 0f);

            Quaternion lookRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = _body.position + lookRotation * _config.CameraOffset;
            transform.rotation = Quaternion.LookRotation(_body.position - transform.position, Vector3.up);
        }

    }
}
