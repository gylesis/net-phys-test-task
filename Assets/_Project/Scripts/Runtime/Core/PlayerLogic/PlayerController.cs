using System;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core.CameraLogic;
using Scripts.Runtime.Core.PhysItems;
using Scripts.Runtime.Services;
using Scripts.Runtime.UI;
using Scripts.Runtime.Utils;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace Scripts.Runtime.Core.PlayerLogic
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkContext
    {
        public static PlayerController LocalPlayer { get; private set; }
        private static Subject<PlayerController> LocalPlayerSpawned { get; } = new Subject<PlayerController>();
        
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Transform _itemDragPoint;
        [SerializeField] private PlayerVisuals _playerVisuals;
        
        public NetworkVariable<bool> IsPaused { get; private set; } = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Owner);
        public NetworkVariable<NetworkBehaviourReference> HoldingItem { get; set; } = new NetworkVariable<NetworkBehaviourReference>();
        public bool IsHoldingItem => HoldingItem.Value.TryGet(out _);

        private float _verticalVelocity;
        private PhysItem _heldItem;
        private Rigidbody _heldBody;
        private Vector3 _grabLocalPoint;

        private PlayerConfig _config;
        private InputService _input;
        private CameraController _cameraController;
        private InteractionSystem _interactionSystem;

        [Inject]
        private void Construct(GameConfig config, InputService input, CameraController cameraController, InteractionSystem interactionSystem)
        {
            _interactionSystem = interactionSystem;
            _config = config.PlayerConfig;
            _input = input;
            _cameraController = cameraController;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            IsPaused.OnValueChanged += HandlePauseChanged;
            HoldingItem.OnValueChanged += HandlePhysItemChanged;
            
            if (IsOwner)
            {
                LocalPlayer = this;
                LocalPlayerSpawned.OnNext(this);
                
                _cameraController.Attach(this);
                SetCursorLocked(true);
            }
        }

        private void HandlePhysItemChanged(NetworkBehaviourReference previousvalue, NetworkBehaviourReference newvalue)
        {
            if (newvalue.TryGet(out PhysItem item))
            {
                _heldItem = item;
                _heldBody = _heldItem.Body;
                _grabLocalPoint = _heldBody.transform.InverseTransformPoint(_heldItem.Body.ClosestPointOnBounds(_itemDragPoint.position));
            }
            else
            {
                ReleaseItem();
            }
        }

        private void HandlePauseChanged(bool previousvalue, bool isPaused)
        {
            if (!IsOwner)
                return;
            
            SetCursorLocked(!isPaused);
            
            if (isPaused)
            {
                UIController.Instance.HideMenu<GameplayHUDMenu>();
                UIController.Instance.ShowMenu<GameplayPauseMenu>();
            }
            else
            {
                UIController.Instance.HideMenu<GameplayPauseMenu>();
                UIController.Instance.ShowMenu<GameplayHUDMenu>();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (_heldItem != null)
                DropItem();

            IsPaused.OnValueChanged -= HandlePauseChanged;
            HoldingItem.OnValueChanged -= HandlePhysItemChanged;

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

            if (_input.PausePressedOnce)
            {
                IsPaused.Value = !IsPaused.Value;
            }

            CheckForInteract();
            UpdateMovement();
        }

        private void FixedUpdate()
        {
            if (!IsOwner || !IsSpawned)
                return;

            if (IsHoldingItem && _heldItem.IsOwner)
                UpdateDraggedItem();
        }

        private void CheckForInteract()
        {
            if (IsPaused.Value)
                return;
            
            if (_input.InteractPressedOnce)
            {
                if (IsHoldingItem)
                {
                    DropItem();
                }
                else
                {
                    var item = _interactionSystem.CurrentNearbyItem.Value;
                    if (item == null || item.IsHeld.Value)
                        return;
                    
                    TryTakeItemRpc(item);
                }
            }
            
            if (IsHoldingItem)
            {
                if (_input.ThrowPressedOnce)
                    ThrowItem();
                else if (_input.DeletePressedOnce)
                    DeleteHoldingItemRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void TryTakeItemRpc(NetworkBehaviourReference itemRef)
        {
            itemRef.TryGet(out PhysItem item);
            
            if (item == null || item.IsHeld.Value)
                return;

            if ((item.transform.position - transform.position).sqrMagnitude > _config.ItemSpring * _config.ItemSpring)
                return;
            
            item.IsHeld.Value = true;
            item.NetworkObject.ChangeOwnership(OwnerClientId);
            
            HoldingItem.Value = item;
        }

        [Rpc(SendTo.Server)]
        private void DropItemRpc()
        {
            if (!HoldingItem.Value.TryGet(out PhysItem item)) 
                return;
            
            item.IsHeld.Value = false;
            item.NetworkObject.RemoveOwnership();
            HoldingItem.Value = null;
        }

        private void ThrowItem()
        {
            Vector3 throwVelocity = _cameraController.transform.forward * _config.ItemThrowForce + _characterController.velocity;
            DropItem(throwVelocity);
        }

        private void DropItem(Vector3 throwVelocity = default)
        {
            _heldItem.Body.linearVelocity = throwVelocity;
            DropItemRpc();
        }

        [Rpc(SendTo.Server)]
        private void DeleteHoldingItemRpc()
        {
            if (!HoldingItem.Value.TryGet(out PhysItem item)) 
                return;
            
            item.NetworkObject.Despawn();
            HoldingItem.Value = null;
        }

        private void ReleaseItem()
        {
            _heldItem = null;
            _heldBody = null;
        }

        private void UpdateDraggedItem()
        {
            Vector3 offset = _itemDragPoint.position - _heldBody.position;
            Vector3 relativeVelocity = _heldBody.linearVelocity - _characterController.velocity;
            Vector3 force = offset * _config.ItemSpring - relativeVelocity * _config.ItemDamping;
            force += _characterController.velocity * _config.ItemCompensateVelocityModifier;

            Vector3 forcePoint = _heldBody.transform.TransformPoint(_grabLocalPoint);
            _heldBody.AddForceAtPosition(force, forcePoint, ForceMode.Acceleration);
        }

        private void UpdateMovement()
        {
            Vector2 input = _input.Move;
            Vector3 planar = Quaternion.Euler(0f, _cameraController.Yaw, 0f) * new Vector3(input.x, 0f, input.y);
            planar = Vector3.ClampMagnitude(planar, 1f);

            float speed = _config.MoveSpeed * (_input.SprintPressed ? _config.SprintMultiplier : 1f);

            if (planar.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(planar);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    _config.RotationSpeed * Time.deltaTime);
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

            if (IsPaused.Value)
            {
                planar = Vector3.zero;
            }
            
            Vector3 velocity = planar * speed + Vector3.up * _verticalVelocity;
            _characterController.Move(velocity * Time.deltaTime);
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public static IDisposable LocalPlayerSpawnedCallback(Action<PlayerController> callback)
        {
            return LocalPlayerSpawned?.Subscribe(callback);
        }
    }
}

