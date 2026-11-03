using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Runtime.Utils
{
    public class DefaultReactiveButton : MonoBehaviour
    {
        [SerializeField] protected Button _button;

        protected IDisposable _clickDisposable;

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
            _clickDisposable?.Dispose();
        }
    }
    
    [RequireComponent(typeof(Button))]
    public abstract class ReactiveButton<TValue> : MonoBehaviour
    {
        [SerializeField] protected Image _image;
        [SerializeField] protected Button _button;

        protected abstract TValue Value { get; }

        private void Reset() =>
            _button = GetComponent<Button>();

        public Subject<EventContext<TValue>> Clicked =
            new Subject<EventContext<TValue>>();

        private IDisposable _disposable;

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ReactiveButton<TSender, TValue> : MonoBehaviour
    {
        [SerializeField] protected Image _image;
        [SerializeField] protected Button _button;

        private IDisposable _disposable;

        protected abstract TValue Value { get; }
        protected abstract TSender Sender { get; }

        protected virtual void Reset()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
        }

        public Subject<EventContext<TSender, TValue>> Clicked { get; } = new Subject<EventContext<TSender, TValue>>();

        protected virtual void Awake()
        {
            /*_disposable = _button.onClick
                .AsObservable()
                .Subscribe((_ =>
                {
                    EventContext<TSender, TValue> buttonContext = new EventContext<TSender, TValue>(Sender, Value);

                    Clicked.OnNext(buttonContext);
                }));*/
        }

        private void OnDestroy() =>
            _disposable.Dispose();
    }

    public struct EventContext<T1, T2>
    {
        public T1 Sender { get; }
        public T2 Value { get; }

        public EventContext(T1 sender, T2 value)
        {
            Sender = sender;
            Value = value;
        }
    }

    public struct EventContext<T1>
    {
        public T1 Value { get; }

        public EventContext(T1 value)
        {
            Value = value;
        }
    }
    
}