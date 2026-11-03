using System.Collections.Generic;
using Scripts.Runtime.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Scripts.Runtime.Core.States
{
    public class LoadGameplayState : GameState
    {
        public override void Enter()
        {
            UIController.Instance.HideAllActiveMenus();

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

        public override void Tick() { }

        public override void Exit() { }
    }
}
