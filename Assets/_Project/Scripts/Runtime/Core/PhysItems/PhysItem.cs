using Scripts.Runtime.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Runtime.Core.PhysItems
{
    public class PhysItem : NetworkContext
    {
        [SerializeField] private Rigidbody _rigidbody;
        
        public Rigidbody Body => _rigidbody;
        
        public NetworkVariable<bool> IsHeld = new NetworkVariable<bool>(false);
    }
}
