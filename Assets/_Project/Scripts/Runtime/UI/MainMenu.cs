using Cysharp.Threading.Tasks;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Utils;
using TMPro;
using UniRx;
using Unity.Netcode;
using UnityEngine;

namespace Scripts.Runtime.UI
{
    public class MainMenu : Menu
    {
        [SerializeField] private DefaultTextReactiveButton _hostBtn;
        [SerializeField] private DefaultTextReactiveButton _joinBtn;

        [SerializeField] private TMP_InputField _ipInput;
        [SerializeField] private TMP_InputField _portInput;
        
        private UniTaskCompletionSource<bool> _connectCts;
        
        protected override void Awake()
        {
            base.Awake();

            _hostBtn.Clicked.Subscribe(unit => HandleHostBtnClicked()).AddTo(destroyCancellationToken);
            _joinBtn.Clicked.Subscribe(unit => HandleJoinBtnClicked()).AddTo(destroyCancellationToken);
        }

        private void HandleJoinBtnClicked()
        {
            TryJoin().Forget();
        }

        private async UniTask TryJoin()
        {
            if (!ushort.TryParse(_portInput.text, out ushort port))
                return;
            
            DisableCanvasGroup();
            _connectCts = new UniTaskCompletionSource<bool>();
            
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            var started = ConnectionManager.Instance.StartClient(_ipInput.text, port);

            if (!started)
            {
                Debug.LogError($"Some error happened");
                return;
            }
            
            var connectCtsTask = await _connectCts.Task;
            EnableCanvasGroup();

            if (!connectCtsTask)
            {
                Debug.LogError($"Error connecting to the server");
                return;
            }

            UIController.Instance.HideMenu<MainMenu>();
        }

        private void HandleClientConnected(ulong playerId)
        {
            _connectCts.TrySetResult(true);
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        private void HandleClientDisconnected(ulong playerId)
        {
            _connectCts.TrySetResult(false);
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        private void HandleHostBtnClicked()
        {
            var hosted = ConnectionManager.Instance.StartHost();

            if (hosted)
            {
                UIController.Instance.HideMenu<MainMenu>();
                UIController.Instance.ShowMenu<LobbyMenu>();
            }
            
        }

        private void OnDestroy()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }
}