using System;
using UniRx;

namespace Scripts.Runtime.Core.PlayerLogic
{
    public class LocalPlayerContext
    {
        public PlayerController Current { get; private set; }

        public IObservable<PlayerController> Changed => _changed;

        private readonly Subject<PlayerController> _changed = new Subject<PlayerController>();

        public void Set(PlayerController player)
        {
            Current = player;
            _changed.OnNext(player);
        }

        public void Clear(PlayerController player)
        {
            if (Current != player)
                return;

            Current = null;
            _changed.OnNext(null);
        }
    }
}
