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
            
            var started = await ConnectionManager.Instance.StartClient(_ipInput.text, port);

            EnableCanvasGroup();
            
            if (!started)
            {
                UIController.Instance.ShowInfoPopUp(Constants.PopUps.ErrorTitle, $"Error connecting to the server: {ConnectionManager.Instance.DisconnectReason}");
                return;
            }

            UIController.Instance.HideMenu<MainMenu>();
            UIController.Instance.ShowMenu<LobbyMenu>();
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
           
        }
    }
}