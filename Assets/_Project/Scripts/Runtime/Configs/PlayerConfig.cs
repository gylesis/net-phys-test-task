using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.Utils;
using UnityEngine;

namespace Scripts.Runtime.Configs
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "NetPhys/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [SerializeField] private PlayerController _playerPrefab;
        
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _sprintMultiplier = 1.6f;
        [SerializeField] private float _jumpVelocity = 5f;
        [SerializeField] private float _gravity = -18f;
        [SerializeField] private float _groundStickVelocity = 1f;

        [Header("Camera")]
        [SerializeField] private float _lookSensitivity = 0.15f;
        [SerializeField] private float _minPitch = -40f;
        [SerializeField] private float _maxPitch = 70f;
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f);

        [Header("Input")]
        [SerializeField] private BetterActionReference _move;
        [SerializeField] private BetterActionReference _look;
        [SerializeField] private BetterActionReference _jump;
        [SerializeField] private BetterActionReference _sprint;

        public PlayerController PlayerPrefab => _playerPrefab;
        public float MoveSpeed => _moveSpeed;

        public float SprintMultiplier => _sprintMultiplier;

        public float JumpVelocity => _jumpVelocity;

        public float Gravity => _gravity;

        public float GroundStickVelocity => _groundStickVelocity;

        public float LookSensitivity => _lookSensitivity;

        public float MinPitch => _minPitch;

        public float MaxPitch => _maxPitch;

        public Vector3 CameraOffset => _cameraOffset;

        public BetterActionReference Move => _move;
        public BetterActionReference Look => _look;

        public BetterActionReference Jump => _jump;

        public BetterActionReference Sprint => _sprint;
    }
}
