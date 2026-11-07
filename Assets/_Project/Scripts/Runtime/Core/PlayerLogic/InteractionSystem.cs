using System;
using Scripts.Runtime.Configs;
using Scripts.Runtime.Core.PhysItems;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Scripts.Runtime.Core.PlayerLogic
{
    public class InteractionSystem : ITickable, IInitializable, IDisposable
    {
        private static readonly Collider[] _hits = new Collider[16];
        public IReadOnlyReactiveProperty<PhysItem> CurrentNearbyItem => _currentNearbyItem;
        
        private readonly CompositeDisposable _disposable = new CompositeDisposable();
        private readonly ReactiveProperty<PhysItem> _currentNearbyItem = new ReactiveProperty<PhysItem>(null);
        private PlayerController _playerController;
        
        private readonly PlayerConfig _config;
        private readonly LocalPlayerContext _localPlayerContext;

        public InteractionSystem(GameConfig config, LocalPlayerContext localPlayerContext)
        {
            _localPlayerContext = localPlayerContext;
            _config = config.PlayerConfig;
        }

        public void Initialize()
        {
            _playerController = _localPlayerContext.Current;
            _localPlayerContext.Changed.Subscribe(OnLocalPlayerChanged).AddTo(_disposable);
        }

        private void OnLocalPlayerChanged(PlayerController player)
        {
            _playerController = player;
        }

        public void Tick()
        {
            if (_playerController == null)
                return;

            var nearest = FindNearestItem();
            if (_currentNearbyItem.Value != nearest)
                _currentNearbyItem.Value = nearest;
        }

        private PhysItem FindNearestItem()
        {
            Vector3 center = _playerController.transform.position;
            int count = Physics.OverlapSphereNonAlloc(center, _config.InteractionRadius, _hits);

            PhysItem best = null;
            float bestSqrDistance = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var collider = _hits[i];
                if (collider == null)
                    continue;
                
                var item = collider.GetComponent<PhysItem>();
                if (item == null || !item.IsSpawned || item.IsHeld.Value || item == best)
                    continue;

                float sqrDistance = (item.transform.position - center).sqrMagnitude;
                if (sqrDistance >= bestSqrDistance)
                    continue;

                bestSqrDistance = sqrDistance;
                best = item;
            }

            return best;
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}
