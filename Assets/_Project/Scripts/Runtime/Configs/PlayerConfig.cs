using System.Collections.Generic;
using Scripts.Runtime.Core.PhysItems;
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
        [SerializeField] private float _rotationSpeed = 10;
        
        [Header("Camera")]
        [SerializeField] private float _lookSensitivity = 0.15f;
        [SerializeField] private float _minPitch = -40f;
        [SerializeField] private float _maxPitch = 70f;
        [SerializeField] private Vector3 _cameraOffset = new Vector3(0f, 2f, -4f);

        [Header("Interaction")]
        [SerializeField] private PhysItem _physItemPrefab;
        [SerializeField] private float _interactionRadius = 2.5f;
        [SerializeField] private float _itemSpring = 150f;
        [SerializeField] private float _itemDamping = 28f;
        [SerializeField] private float _itemThrowForce = 8f;
        [SerializeField] private float _itemCompensateVelocityModifier = 1.3f;
        [SerializeField] private float _itemSpawnDistance = 1.5f;

        [Header("Input")]
        [SerializeField] private BetterActionReference _move;
        [SerializeField] private BetterActionReference _look;
        [SerializeField] private BetterActionReference _jump;
        [SerializeField] private BetterActionReference _sprint;
        [SerializeField] private BetterActionReference _interact;
        [SerializeField] private BetterActionReference _throw;
        [SerializeField] private BetterActionReference _deleteItem;
        [SerializeField] private BetterActionReference _spawn;
        [SerializeField] private BetterActionReference _pause;

        [Header("Visuals")] 
        [SerializeField] private List<PlayerVisualData> _playerVisualDatas;

        public List<PlayerVisualData> PlayerVisualDatas => _playerVisualDatas;
        public PlayerController PlayerPrefab => _playerPrefab;
        public float MoveSpeed => _moveSpeed;
        
        public float ItemCompensateVelocityModifier => _itemCompensateVelocityModifier;

        public float RotationSpeed => _rotationSpeed;
        public float SprintMultiplier => _sprintMultiplier;

        public float JumpVelocity => _jumpVelocity;

        public float Gravity => _gravity;

        public float GroundStickVelocity => _groundStickVelocity;

        public float LookSensitivity => _lookSensitivity;

        public float MinPitch => _minPitch;

        public float MaxPitch => _maxPitch;

        public Vector3 CameraOffset => _cameraOffset;

        public float InteractionRadius => _interactionRadius;

        public PhysItem PhysItemPrefab => _physItemPrefab;

        public float ItemSpawnDistance => _itemSpawnDistance;

        public float ItemSpring => _itemSpring;

        public float ItemDamping => _itemDamping;

        public float ItemThrowForce => _itemThrowForce;

        public BetterActionReference Move => _move;
        public BetterActionReference Look => _look;
        public BetterActionReference Jump => _jump;
        public BetterActionReference Sprint => _sprint;
        public BetterActionReference Interact => _interact;
        public BetterActionReference Throw => _throw;
        public BetterActionReference DeleteItem => _deleteItem;
        public BetterActionReference Spawn => _spawn;
        public BetterActionReference Pause => _pause;
    }
}
