using Cysharp.Threading.Tasks;
using Scripts.Runtime.Core.Net;
using Scripts.Runtime.Core.PlayerLogic;
using Scripts.Runtime.Utils;
using UniRx;
using UnityEngine;

namespace Scripts.Runtime.UI
{
    public class GameplayPauseMenu : Menu
    {
        [SerializeField] private DefaultReactiveButton _exitBtn;
        [SerializeField] private DefaultReactiveButton _resumeBtn;

        protected override void Awake()
        {
            base.Awake();

            _resumeBtn.Clicked.Subscribe(unit => HandleResumeBtnClicked()).AddTo(destroyCancellationToken);
            _exitBtn.Clicked.Subscribe(unit => HandleExitBtnClicked()).AddTo(destroyCancellationToken);
        }

        private void HandleExitBtnClicked()
        {
            ConnectionManager.Instance.LeaveGame();
        }

        private void HandleResumeBtnClicked()
        {
            UIController.Instance.HideMenu<GameplayPauseMenu>();
            UIController.Instance.ShowMenu<GameplayHUDMenu>();
            
            PlayerController.LocalPlayer.IsPaused.Value = false;
        }
    }
}