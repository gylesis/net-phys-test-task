using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Runtime.Utils
{
    public class DefaultReactiveButton : MonoBehaviour
    {
        [SerializeField] protected Button _button;

        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        public Subject<Unit> Clicked { get; } = new Subject<Unit>();

        protected virtual void Awake()
        {
            _button.onClick.AddListener(() =>
            {
                Clicked.OnNext(Unit.Default);
            });
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }
    }
}