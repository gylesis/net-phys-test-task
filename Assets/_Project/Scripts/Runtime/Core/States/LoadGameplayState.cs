using System.Collections.Generic;
using Scripts.Runtime.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Scripts.Runtime.Core.States
{
    public class LoadGameplayState : GameState
    {
        private readonly UIController _uiController;
        private readonly Curtains _curtains;

        public LoadGameplayState(UIController uiController, Curtains curtains)
        {
            _curtains = curtains;
            _uiController = uiController;
        }

        public override void Enter()
        {
            _curtains.ShowCurtains();
            _uiController.HideAllActiveMenus();

            var sceneManager = NetworkManager.Singleton.SceneManager;
            sceneManager.OnLoadEventCompleted += HandleLoadEventCompleted;

            if (NetworkManager.Singleton.IsServer)
                sceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }

        private void HandleLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
            List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (sceneName != "Gameplay")
                return;

            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= HandleLoadEventCompleted;
            Machine.Enter<GameplayState>();
        }

        public override void Exit() { }
    }
}
