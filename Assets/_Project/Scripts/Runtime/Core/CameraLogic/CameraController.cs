using Scripts.Runtime.Configs;
using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Scripts.Runtime.Core.CameraLogic
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform _camParent;
        
        private PlayerConfig _config;
        private InputService _input;

        private Transform _body;
        private float _yaw;
        private float _pitch;
        private PlayerController _attachedPlayer;

        public float Yaw => _yaw;

        [Inject]
        private void Construct(GameConfig config, InputService input)
        {
            _config = config.PlayerConfig;
            _input = input;
        }

        public void Attach(PlayerController playerController)
        {
            _attachedPlayer = playerController;
            _body = playerController.transform;
            _yaw = _body != null ? _body.eulerAngles.y : 0f;
            _pitch = 0f;
        }
        
        private void LateUpdate()
        {
            if (!_attachedPlayer) 
                return;

            Vector2 delta = _input.Look;

            if (_attachedPlayer.IsPaused.Value)
            {
                delta = Vector2.zero;
            }
            
            _yaw += delta.x * _config.LookSensitivity;
            _pitch = Mathf.Clamp(_pitch - delta.y * _config.LookSensitivity, _config.MinPitch, _config.MaxPitch);

            transform.SetPositionAndRotation(_body.position, Quaternion.Euler(0f, _yaw, 0f));

            Quaternion pitchRotation = Quaternion.Euler(_pitch, 0f, 0f);
            _camParent.localPosition = pitchRotation * _config.CameraOffset;
            _camParent.localRotation = pitchRotation;
        }

    }
}
